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

var webRootPath =
  app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
var imageDirectory = Path.Combine(webRootPath, "images");
Directory.CreateDirectory(imageDirectory);

var allowedImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
  ".jpg",
  ".jpeg",
  ".png",
};

var contentTypesByExtension = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
  [".jpg"] = "image/jpeg",
  [".jpeg"] = "image/jpeg",
  [".png"] = "image/png",
};



app.UseStaticFiles();

app.MapGet(
  "/image-gallery",
  () =>
  {
    var images = Directory
      .GetFiles(imageDirectory)
      .Select(Path.GetFileName)
      .Where(fileName => !string.IsNullOrWhiteSpace(fileName))
      .Cast<string>()
      .Where(fileName => allowedImageExtensions.Contains(Path.GetExtension(fileName)))
      .OrderBy(fileName => fileName, StringComparer.OrdinalIgnoreCase)
      .Select(fileName => new ImageResult(
        $"/image/get/{Uri.EscapeDataString(fileName)}",
        Path.GetFileNameWithoutExtension(fileName)
      ))
      .ToList();

    return Results.Ok(new ImageGalleryResult(images));
  }
);

app.MapGet(
  "/image/get/{imageName}",
  (string imageName) =>
  {
    var safeImageName = Path.GetFileName(imageName);
    var filePath = Path.Combine(imageDirectory, safeImageName);

    if (string.IsNullOrWhiteSpace(safeImageName) || !File.Exists(filePath))
    {
      return Results.NotFound();
    }

    var extension = Path.GetExtension(safeImageName);
    var contentType = contentTypesByExtension.GetValueOrDefault(
      extension,
      "application/octet-stream"
    );

    return Results.File(filePath, contentType);
  }
);

app.MapPost(
  "/image/upload",
  async (HttpRequest request) =>
  {
    if (!request.HasFormContentType)
    {
      return Results.BadRequest("Multipart form data expected.");
    }

    var form = await request.ReadFormAsync();

    foreach (var file in form.Files)
    {
      if (file.Length <= 0)
      {
        continue;
      }

      var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

      if (!allowedImageExtensions.Contains(extension))
      {
        return Results.BadRequest($"Unsupported file extension '{extension}'.");
      }

      var fileName = $"{Guid.NewGuid():N}{extension}";
      var filePath = Path.Combine(imageDirectory, fileName);

      await using var stream = File.Create(filePath);
      await file.CopyToAsync(stream);
    }

    return Results.Ok();
  }
);

app.MapDelete(
  "/image/delete/{imageName}",
  (string imageName) =>
  {
    var safeImageName = Path.GetFileName(imageName);
    var filePath = Path.Combine(imageDirectory, safeImageName);

    if (string.IsNullOrWhiteSpace(safeImageName) || !File.Exists(filePath))
    {
      return Results.NotFound();
    }

    try
    {
      File.Delete(filePath);
      return Results.Ok();
    }
    catch
    {
      return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
  }
);

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

internal sealed record ImageResult(string url, string name);

internal sealed record ImageGalleryResult(IReadOnlyList<ImageResult> images);

internal sealed record RequestUpdate(DateOnly until, RequestStatus status);
