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
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ActiveSession", policy =>
        policy.RequireAuthenticatedUser().RequireAssertion(context =>
            context.User.FindFirst("mustChangePassword")?.Value != "True"));
});

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
    if (employee is null || !employee.isActive ||
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

    if (request.newPassword.Length < 8)
    {
        return Results.BadRequest(new { message = "Neues Passwort muss mindestens 8 Zeichen lang sein." });
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

app.MapPost("/api/employees", async (CreateEmployeeRequest request, EmployeeStore store, IConfiguration config) =>
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
        permissionLevel = request.permissionLevel,
        mustChangePassword = true,
    };
    employee.passwordHash = new PasswordHasher<Employee>().HashPassword(employee, defaultPassword);

    var created = await store.CreateAsync(employee);
    return Results.Created($"/api/employees/{created.id}", EmployeeResponse.From(created));
}).RequireAuthorization("ActiveSession");

app.MapPut("/api/employees/{id}/toggle-active", async (Guid id, EmployeeStore store) =>
    await store.ToggleActiveAsync(id) ? Results.Ok() : Results.NotFound())
    .RequireAuthorization("ActiveSession");

app.MapPut("/api/employees/{id}", async (Guid id, UpdateEmployeeRequest request, EmployeeStore store) =>
    await store.UpdateAsync(id, request) ? Results.Ok() : Results.NotFound())
    .RequireAuthorization("ActiveSession");

app.MapDelete("/api/employees/{id}", (Guid id, EmployeeStore store) =>
    store.Delete(id) ? Results.Ok() : Results.NotFound());

app.MapGet("/api/requests", async (string? status, RequestsStore store) =>
{
    var requests = status == "open" ? await store.GetOpenAsync() : await store.AllAsync();
    return Results.Ok(requests);
}).RequireAuthorization("ActiveSession");

app.MapPost("/api/requests", async (Request request, RequestsStore store) =>
{
    if (request.employeeId == Guid.Empty)
    {
        return Results.BadRequest(new { message = "employeeId ist erforderlich." });
    }

    var created = await store.CreateAsync(request);
    return Results.Created($"/api/requests/{created.id}", created);
}).RequireAuthorization("ActiveSession");

// Enddatum + Status ändern (z.B. aus einem Bearbeiten-Dialog), unabhängig von approve/reject
app.MapPut("/api/requests/{id}", async (Guid id, RequestUpdate update, RequestsStore store) =>
    await store.UpdateAsync(id, update.until, update.status) ? Results.Ok() : Results.NotFound())
    .RequireAuthorization("ActiveSession");

app.MapPut("/api/requests/{id}/approve", async (Guid id, RequestsStore store) =>
    await store.SetStatusAsync(id, RequestStatus.Approved) ? Results.Ok() : Results.NotFound())
    .RequireAuthorization("ActiveSession");

app.MapPut("/api/requests/{id}/reject", async (Guid id, RequestsStore store) =>
    await store.SetStatusAsync(id, RequestStatus.Rejected) ? Results.Ok() : Results.NotFound())
    .RequireAuthorization("ActiveSession");

app.MapDelete("/api/requests/{id}", async (Guid id, RequestsStore store) =>
    await store.RemoveAsync(id) ? Results.Ok() : Results.NotFound())
    .RequireAuthorization("ActiveSession");

app.MapGet("/api/auditlog", async (AuditLogStore store) =>
    Results.Ok(await store.AllAsync()))
    .RequireAuthorization("ActiveSession");

// Bewusst kein PUT/DELETE für Audit-Log-Einträge: nachträgliches Ändern/Löschen
// würde die geforderte Revisionssicherheit (BR-01.07) untergraben.
app.MapPost("/api/auditlog", async (CreateAuditLogRequest request, ClaimsPrincipal user, AuditLogStore store) =>
{
    var entry = new AuditLogEntry
    {
        Action = request.Action,
        Summary = request.Summary,
        Reference = request.Reference,
        Actor = user.FindFirst(ClaimTypes.Name)!.Value,
        Timestamp = DateTime.UtcNow,
    };
    var created = await store.CreateAsync(entry);
    return Results.Created($"/api/auditlog/{created.Id}", created);
}).RequireAuthorization("ActiveSession");

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

internal sealed record EmployeeResponse(
    Guid id, string name, string role, int workload, double vacationDays, bool isActive,
    string username, int permissionLevel, bool mustChangePassword)
{
    public static EmployeeResponse From(Employee e) => new(
        e.id, e.name, e.role, e.workload, e.vacationDays, e.isActive,
        e.username, e.permissionLevel, e.mustChangePassword);
}

internal sealed record CreateEmployeeRequest(
    string name, string role, int workload, double vacationDays, string username, int permissionLevel);

public sealed record UpdateEmployeeRequest(
    string name, string role, int workload, double vacationDays, int permissionLevel);

internal sealed record CreateAuditLogRequest(AuditLogAction Action, string Summary, Guid Reference);
