using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

using Path = System.IO.Path;

namespace Deploy.Tasks;

public class DeployArtifactsTask : FrostingTask<DeployContext>
{
  private static readonly HashSet<string> TargetRootWhitelist = new(
    [".git", ".nojekyll"],
    StringComparer.OrdinalIgnoreCase
  );

  public override void Run(DeployContext context)
  {
    if (context.TargetPath is null)
    {
      throw new CakeException("The TargetPath argument is required.");
    }

    if (!File.Exists(context.WebArchivePath.FullPath))
    {
      throw new CakeException(
        $"Web archive not found: '{context.WebArchivePath.FullPath}'."
      );
    }

    context.Information($"*** Deploying artifacts to {context.TargetPath.FullPath}...");
    context.EnsureDirectoryExists(context.TargetPath);
    CleanTargetDirectory(context.TargetPath);

    ZipFile.ExtractToDirectory(
      context.WebArchivePath.FullPath,
      context.TargetPath.FullPath,
      overwriteFiles: true
    );
  }

  private static void CleanTargetDirectory(DirectoryPath targetDir)
  {
    foreach (string entryPath in Directory.EnumerateFileSystemEntries(targetDir.FullPath))
    {
      string entryName = Path.GetFileName(entryPath);

      if (TargetRootWhitelist.Contains(entryName))
      {
        continue;
      }

      if (Directory.Exists(entryPath))
      {
        Directory.Delete(entryPath, recursive: true);
      }
      else
      {
        File.Delete(entryPath);
      }
    }
  }

}
