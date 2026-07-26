using FullstackRessourcix;

using Mumrich.SpaDevMiddleware.Extensions;

var builder = WebApplication.CreateBuilder(args);
var appSettings = builder.Configuration.Get<AppSettings>();

ArgumentNullException.ThrowIfNull(appSettings);

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

app.MapSinglePageApps(appSettings);

app.Run();

internal sealed record ImageResult(string url, string name);

internal sealed record ImageGalleryResult(IReadOnlyList<ImageResult> images);
