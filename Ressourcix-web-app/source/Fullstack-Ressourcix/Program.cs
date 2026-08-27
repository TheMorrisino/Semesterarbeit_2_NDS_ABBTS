using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using FullstackRessourcix;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Mumrich.SpaDevMiddleware.Extensions;

var builder = WebApplication.CreateBuilder(args);
var appSettings = builder.Configuration.Get<AppSettings>();

ArgumentNullException.ThrowIfNull(appSettings);

var connectionString =
  builder.Configuration.GetConnectionString("AppDb")
  ?? throw new InvalidOperationException("ConnectionStrings:AppDb ist nicht konfiguriert.");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder
  .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie(options =>
  {
    options.Cookie.Name = "ressourcix.auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
      context.Response.StatusCode = StatusCodes.Status401Unauthorized;
      return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
      context.Response.StatusCode = StatusCodes.Status403Forbidden;
      return Task.CompletedTask;
    };
    // Login prüft IsActive nur einmal - ohne diese erneute Prüfung pro Request bliebe eine
    // bereits ausgestellte Session bis zu 8h gültig, selbst wenn das Konto zwischenzeitlich
    // deaktiviert wurde (siehe AuthStore.IsActiveAsync).
    options.Events.OnValidatePrincipal = async context =>
    {
      var employeeIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var authStore = context.HttpContext.RequestServices.GetRequiredService<AuthStore>();
      if (!Guid.TryParse(employeeIdClaim, out var employeeId) || !await authStore.IsActiveAsync(employeeId))
      {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
      }
    };
  });
builder.Services.AddAuthorization(options =>
{
  options.AddPolicy(
    "ActiveSession",
    policy =>
      policy
        .RequireAuthenticatedUser()
        .RequireAssertion(context => context.User.FindFirst("mustChangePassword")?.Value != "True")
  );

  options.AddPolicy(
    "Admin",
    policy =>
      policy
        .RequireAuthenticatedUser()
        .RequireAssertion(context =>
          context.User.FindFirst("mustChangePassword")?.Value != "True"
          && int.TryParse(context.User.FindFirst("permissionLevel")?.Value, out var level)
          && level >= 5
        )
  );
});

// Bremst Brute-Force-Passwortraten gegen bekannte Benutzernamen aus: max. 5 Login-Versuche
// pro Minute und IP, danach 429 statt einer weiteren Prüfung.
builder.Services.AddRateLimiter(options =>
{
  options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
  options.OnRejected = (context, cancellationToken) =>
  {
    context.HttpContext.Response.ContentType = "application/json";
    return new ValueTask(
      context.HttpContext.Response.WriteAsJsonAsync(
        new { message = "Zu viele Login-Versuche. Bitte versuche es in einer Minute erneut." },
        cancellationToken
      )
    );
  };
  options.AddPolicy(
    "login",
    httpContext =>
      RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
          PermitLimit = 5,
          Window = TimeSpan.FromMinutes(1),
          QueueLimit = 0,
        }
      )
  );
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<PasswordHasher<Employee>>();

builder.Services.AddScoped<AuthStore>();

builder.Services.AddScoped<EmployeeStore>();

builder.Services.AddScoped<RequestsStore>();

builder.Services.AddScoped<AuditLogStore>();

// Enums als lesbare Strings statt nackter Zahlen serialisieren (z.B. "Open" statt 0)
builder.Services.ConfigureHttpJsonOptions(options =>
{
  options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.SetupSpaMiddleware(appSettings);

var app = builder.Build();

app.UseExceptionHandler();

app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
{
  app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapAuthEndpoints();
app.MapEmployeeEndpoints();
app.MapRequestEndpoints();
app.MapAuditLogEndpoints();

app.MapSinglePageApps(appSettings);

app.Run();
