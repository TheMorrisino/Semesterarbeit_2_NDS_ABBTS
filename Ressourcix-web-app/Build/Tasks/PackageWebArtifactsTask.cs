using System.IO;
using System.IO.Compression;

using Cake.Common.Diagnostics;
using Cake.Frosting;

namespace Build.Tasks;

[IsDependentOn(typeof(BuildTeachingAppTask))]
[IsDependentOn(typeof(BuildTeachingNdsWegTask))]
public class PackageWebArtifactsTask : FrostingTask<BuildContext>
{
  public override void Run(BuildContext context)
  {
    context.Information($"*** Creating web archive {context.WebArchivePath.FullPath}...");

    if (File.Exists(context.WebArchivePath.FullPath))
    {
      File.Delete(context.WebArchivePath.FullPath);
    }

    // Create .nojekyll file in the root of web output
    string noJekyllPath = Path.Combine(context.AppPublishDir.FullPath, ".nojekyll");
    context.Information($"*** Creating .nojekyll file at {noJekyllPath}...");
    File.WriteAllText(noJekyllPath, string.Empty);

    ZipFile.CreateFromDirectory(
      context.AppPublishDir.FullPath,
      context.WebArchivePath.FullPath,
      CompressionLevel.Optimal,
      includeBaseDirectory: false
    );
  }
}
