using System.Security.Claims;
using System.Text.Json.Serialization;

using FullstackRessourcix;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Mumrich.SpaDevMiddleware.Extensions;

var builder = WebApplication.CreateBuilder(args);
var appSettings = builder.Configuration.Get<AppSettings>();

ArgumentNullException.ThrowIfNull(appSettings);

var connectionString = builder.Configuration.GetConnectionString("AppDb")
    ?? throw new InvalidOperationException("ConnectionStrings:AppDb ist nicht konfiguriert.");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
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
    });
builder.Services.AddAuthorization();

builder.Services.AddSingleton<EmployeeStore>();

builder.Services.AddSingleton<RequestsStore>();

builder.Services.AddSingleton<AuditLogStore>();

// Enums als lesbare Strings statt nackter Zahlen serialisieren (z.B. "Open" statt 0)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.SetupSpaMiddleware(appSettings);

var app = builder.Build();

app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/login", async (LoginRequest request, AppDbContext db, HttpContext http) =>
{
    var employee = await db.Employees.FirstOrDefaultAsync(e => e.username == request.username);
    var hasher = new PasswordHasher<Employee>();
    if (employee is null ||
        hasher.VerifyHashedPassword(employee, employee.passwordHash, request.password) == PasswordVerificationResult.Failed)
    {
        return Results.Unauthorized();
    }

    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, BuildPrincipal(employee));
    return Results.Ok(AuthUserResponse.From(employee));
});

app.MapGet("/api/auth/me", (ClaimsPrincipal user) =>
    user.Identity?.IsAuthenticated == true
        ? Results.Ok(AuthUserResponse.FromClaims(user))
        : Results.Unauthorized());

app.MapPost("/api/auth/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
}).RequireAuthorization();

app.MapPost("/api/auth/change-password", async (ChangePasswordRequest request, ClaimsPrincipal user, AppDbContext db, HttpContext http) =>
{
    var id = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    var employee = await db.Employees.FindAsync(id);
    if (employee is null) return Results.NotFound();

    var hasher = new PasswordHasher<Employee>();
    if (hasher.VerifyHashedPassword(employee, employee.passwordHash, request.currentPassword) == PasswordVerificationResult.Failed)
    {
        return Results.BadRequest(new { message = "Aktuelles Passwort ist falsch." });
    }

    employee.passwordHash = hasher.HashPassword(employee, request.newPassword);
    employee.mustChangePassword = false;
    await db.SaveChangesAsync();

    // Cookie neu ausstellen, damit mustChangePassword=false sofort im Claim steht
    // (sonst würde der Router-Guard im Frontend weiter auf /change-password umleiten).
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, BuildPrincipal(employee));

    return Results.Ok();
}).RequireAuthorization();

app.MapGet("/api/employees", (EmployeeStore store) =>
    Results.Ok(store.All()));

app.MapPost("/api/employees", (Employee employee, EmployeeStore store) =>
{
    var created = store.Create(employee);
    return Results.Created($"/api/employees/{created.id}", created);
});

app.MapPut("/api/employees/{id}/toggle-active", (Guid id, EmployeeStore store) =>
    store.ToggleActive(id) ? Results.Ok() : Results.NotFound());

app.MapPut("/api/employees/{id}", (Guid id, Employee employee, EmployeeStore store) =>
    store.Update(id, employee) ? Results.Ok() : Results.NotFound());

app.MapGet("/api/requests", (string? status, RequestsStore store) =>
{
    var requests = status == "open" ? store.GetOpen() : store.All();
    return Results.Ok(requests);
});

app.MapPost("/api/requests", (Request request, RequestsStore store) =>
{
    var created = store.Create(request);
    return Results.Created($"/api/requests/{created.id}", created);
});

// Enddatum + Status ändern (z.B. aus einem Bearbeiten-Dialog), unabhängig von approve/reject
app.MapPut("/api/requests/{id}", (Guid id, RequestUpdate update, RequestsStore store) =>
    store.Update(id, update.until, update.status) ? Results.Ok() : Results.NotFound());

app.MapPut("/api/requests/{id}/approve", (Guid id, RequestsStore store) =>
    store.SetStatus(id, RequestStatus.Approved) ? Results.Ok() : Results.NotFound());

app.MapPut("/api/requests/{id}/reject", (Guid id, RequestsStore store) =>
    store.SetStatus(id, RequestStatus.Rejected) ? Results.Ok() : Results.NotFound());

app.MapDelete("/api/requests/{id}", (Guid id, RequestsStore store) =>
    store.Remove(id) ? Results.Ok() : Results.NotFound());

app.MapGet("/api/auditlog", (AuditLogStore store) =>
    Results.Ok(store.All()));

// Bewusst kein PUT/DELETE für Audit-Log-Einträge: nachträgliches Ändern/Löschen
// würde die geforderte Revisionssicherheit (BR-01.07) untergraben.
app.MapPost("/api/auditlog", (AuditLogEntry entry, AuditLogStore store) =>
{
    var created = store.Create(entry);
    return Results.Created($"/api/auditlog/{created.Id}", created);
});

app.MapSinglePageApps(appSettings);

app.Run();

static ClaimsPrincipal BuildPrincipal(Employee employee)
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, employee.id.ToString()),
        new(ClaimTypes.Name, employee.username),
        new("displayName", employee.name),
        new("permissionLevel", employee.permissionLevel.ToString()),
        new("mustChangePassword", employee.mustChangePassword.ToString()),
    };
    return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
}

internal sealed record ImageResult(string url, string name);

internal sealed record ImageGalleryResult(IReadOnlyList<ImageResult> images);

internal sealed record RequestUpdate(DateOnly until, RequestStatus status);

internal sealed record LoginRequest(string username, string password);

internal sealed record ChangePasswordRequest(string currentPassword, string newPassword);

internal sealed record AuthUserResponse(Guid id, string username, string name, int permissionLevel, bool mustChangePassword)
{
    public static AuthUserResponse From(Employee e) =>
        new(e.id, e.username, e.name, e.permissionLevel, e.mustChangePassword);

    public static AuthUserResponse FromClaims(ClaimsPrincipal user) => new(
        Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value),
        user.FindFirst(ClaimTypes.Name)!.Value,
        user.FindFirst("displayName")!.Value,
        int.Parse(user.FindFirst("permissionLevel")!.Value),
        bool.Parse(user.FindFirst("mustChangePassword")!.Value));
}
