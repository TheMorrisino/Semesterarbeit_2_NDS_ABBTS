using System.Text.Json.Serialization;

using FullstackRessourcix;

using Mumrich.SpaDevMiddleware.Extensions;

var builder = WebApplication.CreateBuilder(args);
var appSettings = builder.Configuration.Get<AppSettings>();

ArgumentNullException.ThrowIfNull(appSettings);

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

app.MapDelete("/api/employees/{id}", (Guid id, EmployeeStore store) =>
    store.Delete(id) ? Results.Ok() : Results.NotFound());

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

internal sealed record ImageResult(string url, string name);

internal sealed record ImageGalleryResult(IReadOnlyList<ImageResult> images);

internal sealed record RequestUpdate(DateOnly until, RequestStatus status);
