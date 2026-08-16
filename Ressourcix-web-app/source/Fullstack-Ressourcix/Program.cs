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
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ActiveSession", policy =>
        policy.RequireAuthenticatedUser().RequireAssertion(context =>
            context.User.FindFirst("mustChangePassword")?.Value != "True"));

    options.AddPolicy("Admin", policy =>
        policy.RequireAuthenticatedUser().RequireAssertion(context =>
            context.User.FindFirst("mustChangePassword")?.Value != "True" &&
            int.TryParse(context.User.FindFirst("permissionLevel")?.Value, out var level) &&
            level >= 5));
});

// Bremst Brute-Force-Passwortraten gegen bekannte Benutzernamen aus: max. 5 Login-Versuche
// pro Minute und IP, danach 429 statt einer weiteren Prüfung.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        return new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(
            new { message = "Zu viele Login-Versuche. Bitte versuche es in einer Minute erneut." },
            cancellationToken));
    };
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

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

app.MapPost("/api/auth/login", async (LoginRequest request, AppDbContext db, HttpContext http) =>
{
    var employee = await db.Employees.FirstOrDefaultAsync(e => e.username == request.username);
    var hasher = new PasswordHasher<Employee>();
    if (employee is null || !employee.isActive ||
        hasher.VerifyHashedPassword(employee, employee.passwordHash, request.password) == PasswordVerificationResult.Failed)
    {
        return Results.Unauthorized();
    }

    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, BuildPrincipal(employee));
    return Results.Ok(AuthUserResponse.From(employee));
}).RequireRateLimiting("login");

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

    if (!IsStrongPassword(request.newPassword))
    {
        return Results.BadRequest(new
        {
            message = "Neues Passwort muss mindestens 8 Zeichen lang sein und Gross-/Kleinbuchstaben, " +
                "eine Zahl und ein Sonderzeichen enthalten.",
        });
    }

    if (request.newPassword == request.currentPassword)
    {
        return Results.BadRequest(new { message = "Neues Passwort muss sich vom aktuellen unterscheiden." });
    }

    employee.passwordHash = hasher.HashPassword(employee, request.newPassword);
    employee.mustChangePassword = false;
    await db.SaveChangesAsync();

    // Cookie neu ausstellen, damit mustChangePassword=false sofort im Claim steht
    // (sonst würde der Router-Guard im Frontend weiter auf /change-password umleiten).
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, BuildPrincipal(employee));

    return Results.Ok();
}).RequireAuthorization();

app.MapGet("/api/employees", async (EmployeeStore store) =>
    Results.Ok((await store.AllAsync()).Select(EmployeeResponse.From)))
    .RequireAuthorization("ActiveSession");

app.MapPost("/api/employees", async (CreateEmployeeRequest request, ClaimsPrincipal user, EmployeeStore store, IConfiguration config) =>
{
    if (await store.UsernameExistsAsync(request.username))
    {
        return Results.Conflict(new { message = "Benutzername bereits vergeben." });
    }

    var defaultPassword = config["Auth:DefaultPassword"]
        ?? throw new InvalidOperationException("Auth:DefaultPassword ist nicht konfiguriert.");

    var employee = new Employee
    {
        name = request.name,
        role = request.role,
        workload = request.workload,
        vacationDays = request.vacationDays,
        isActive = true,
        username = request.username,
        permissionLevel = Employee.PermissionLevelForRole(request.role),
        mustChangePassword = true,
    };
    employee.passwordHash = new PasswordHasher<Employee>().HashPassword(employee, defaultPassword);

    var created = await store.CreateAsync(employee, ActorName(user));
    return Results.Created($"/api/employees/{created.id}", EmployeeResponse.From(created));
}).RequireAuthorization("Admin");

app.MapPut("/api/employees/{id}/toggle-active", async (Guid id, ClaimsPrincipal user, EmployeeStore store) =>
    await store.ToggleActiveAsync(id, ActorName(user)) ? Results.Ok() : Results.NotFound())
    .RequireAuthorization("Admin");

app.MapPut("/api/employees/{id}", async (Guid id, UpdateEmployeeRequest request, ClaimsPrincipal user, EmployeeStore store) =>
    await store.UpdateAsync(id, request, ActorName(user)) ? Results.Ok() : Results.NotFound())
    .RequireAuthorization("Admin");

app.MapDelete("/api/employees/{id}", async (Guid id, ClaimsPrincipal user, EmployeeStore store) =>
    await store.DeleteAsync(id, ActorName(user)) ? Results.Ok() : Results.NotFound())
    .RequireAuthorization("Admin");

app.MapGet("/api/requests", async (string? status, RequestsStore store) =>
{
    var requests = status == "open" ? await store.GetOpenAsync() : await store.AllAsync();
    return Results.Ok(requests.Select(RequestResponse.From));
}).RequireAuthorization("ActiveSession");

app.MapPost("/api/requests", async (Request request, ClaimsPrincipal user, RequestsStore store) =>
{
    if (request.employeeId == Guid.Empty)
    {
        return Results.BadRequest(new { message = "employeeId ist erforderlich." });
    }
    if (!CanActOnRequest(user, request.employeeId))
    {
        return Results.Forbid();
    }

    var created = await store.CreateAsync(request, ActorName(user));
    return Results.Created($"/api/requests/{created.id}", RequestResponse.From(created));
}).RequireAuthorization("ActiveSession");

// Enddatum + Status ändern (z.B. aus einem Bearbeiten-Dialog), unabhängig von approve/reject
app.MapPut("/api/requests/{id}", async (Guid id, RequestUpdate update, ClaimsPrincipal user, RequestsStore store) =>
{
    var existing = await store.GetAsync(id);
    if (existing is null) return Results.NotFound();
    if (!CanActOnRequest(user, existing.employeeId))
    {
        return Results.Forbid();
    }

    return await store.UpdateAsync(id, update.until, update.status, IsAdmin(user), ActorName(user)) ? Results.Ok() : Results.NotFound();
}).RequireAuthorization("ActiveSession");

// Keine Frontend-Aufrufer (Approve/Reject läuft über PUT /api/requests/{id}); bewusst beibehalten
// als Admin-API-Surface für direkte/zukünftige Nutzung.
app.MapPut("/api/requests/{id}/approve", async (Guid id, ClaimsPrincipal user, RequestsStore store) =>
    await store.SetStatusAsync(id, RequestStatus.Approved, ActorName(user)) ? Results.Ok() : Results.NotFound())
    .RequireAuthorization("Admin"); // Nur Admins dürfen Anträge genehmigen, nicht normale Mitarbeiter

app.MapPut("/api/requests/{id}/reject", async (Guid id, ClaimsPrincipal user, RequestsStore store) =>
    await store.SetStatusAsync(id, RequestStatus.Rejected, ActorName(user)) ? Results.Ok() : Results.NotFound())
    .RequireAuthorization("Admin"); // Nur Admins dürfen Anträge ablehnen, nicht normale Mitarbeiter

app.MapDelete("/api/requests/{id}", async (Guid id, ClaimsPrincipal user, RequestsStore store) =>
{
    var existing = await store.GetAsync(id);
    if (existing is null) return Results.NotFound();
    if (!CanActOnRequest(user, existing.employeeId))
    {
        return Results.Forbid();
    }

    return await store.RemoveAsync(id, ActorName(user)) ? Results.Ok() : Results.NotFound();
}).RequireAuthorization("ActiveSession");

// Kein POST/PUT/DELETE für Audit-Log-Einträge über die API: Einträge entstehen ausschliesslich
// serverseitig als Nebeneffekt der jeweiligen Mutation in EmployeeStore/RequestsStore, damit die
// geforderte Revisionssicherheit (BR-01.07) nicht durch frei wählbare Client-Werte untergraben wird.
app.MapGet("/api/auditlog", async (AuditLogStore store) =>
    Results.Ok((await store.AllAsync()).Select(AuditLogResponse.From)))
    .RequireAuthorization("ActiveSession");

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

static bool IsAdmin(ClaimsPrincipal user) =>
    int.TryParse(user.FindFirst("permissionLevel")?.Value, out var level) && level >= 5;

static Guid CurrentEmployeeId(ClaimsPrincipal user) =>
    Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);

static bool CanActOnRequest(ClaimsPrincipal user, Guid employeeId) =>
    IsAdmin(user) || employeeId == CurrentEmployeeId(user);

static string ActorName(ClaimsPrincipal user) =>
    user.FindFirst("displayName")!.Value;

static bool IsStrongPassword(string password) =>
    password.Length >= 8 &&
    password.Any(char.IsUpper) &&
    password.Any(char.IsLower) &&
    password.Any(char.IsDigit) &&
    password.Any(ch => !char.IsLetterOrDigit(ch));

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

internal sealed record EmployeeResponse(
    Guid id, string name, string role, int workload, double vacationDays, bool isActive,
    string username, int permissionLevel, bool mustChangePassword)
{
    public static EmployeeResponse From(Employee e) => new(
        e.id, e.name, e.role, e.workload, e.vacationDays, e.isActive,
        e.username, e.permissionLevel, e.mustChangePassword);
}

internal sealed record CreateEmployeeRequest(
    string name, string role, int workload, double vacationDays, string username);

public sealed record UpdateEmployeeRequest(
    string name, string role, int workload, double vacationDays);

internal sealed record RequestResponse(
    Guid id, Guid employeeId, DateOnly from, DateOnly until, int days, bool overlap,
    RequestStatus status, DateTime submittedOn, AbsenceType type, string? remark)
{
    public static RequestResponse From(Request r) => new(
        r.id, r.employeeId, r.from, r.until, r.days, r.overlap,
        r.status, r.submittedOn, r.type, r.remark);
}

internal sealed record AuditLogResponse(
    Guid id, AuditLogAction action, string summary, Guid reference, string actor, DateTime timestamp)
{
    public static AuditLogResponse From(AuditLogEntry e) =>
        new(e.Id, e.Action, e.Summary, e.Reference, e.Actor, e.Timestamp);
}
