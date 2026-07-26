using System.Diagnostics;
using System.Text.Json;

using Cake.Common;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

using Deploy.Tasks;

using Path = System.IO.Path;

namespace Deploy;

public static class Program
{
  public static int Main(string[] args)
  {
    return new CakeHost().UseContext<DeployContext>().Run(args);
  }
}

public class DeployContext : FrostingContext
{
  public DirectoryPath ArtifactsDir { get; }

  public FilePath WebArchivePath { get; }

  public DirectoryPath TargetPath { get; }

  public DeployContext(ICakeContext context)
    : base(context)
  {
    ArtifactsDir = context.Argument(nameof(ArtifactsDir), context.Directory("../.artifacts"));
    TargetPath = context.Argument<DirectoryPath>(nameof(TargetPath), null!);

    string gitVersion = GetGitVersion();
    WebArchivePath = context.Argument<FilePath>(
      nameof(WebArchivePath),
      context.File(Path.Combine(ArtifactsDir.FullPath, $"web-{gitVersion}.zip"))
    );
  }

  private static string GetGitVersion()
  {
    ProcessStartInfo startInfo = new()
    {
      FileName = "gitversion",
      Arguments = "/output json",
      RedirectStandardError = true,
      RedirectStandardOutput = true,
      UseShellExecute = false,
      CreateNoWindow = true,
    };

    using Process process =
      Process.Start(startInfo) ?? throw new CakeException("Could not start GitVersion.");

    string output = process.StandardOutput.ReadToEnd();
    string error = process.StandardError.ReadToEnd();
    process.WaitForExit();

    if (process.ExitCode != 0)
    {
      throw new CakeException($"GitVersion failed: {error}");
    }

    GitVersionOutput gitVersion =
      JsonSerializer.Deserialize<GitVersionOutput>(
        output,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
      ) ?? throw new CakeException("GitVersion did not return a valid payload.");

    return gitVersion.FullSemVer
      ?? throw new CakeException("GitVersion did not return FullSemVer.");
  }

  private sealed class GitVersionOutput
  {
    public string FullSemVer { get; set; } = string.Empty;
  }
}

[TaskName("Default")]
[IsDependentOn(typeof(DeployArtifactsTask))]
public class DefaultTask : FrostingTask { }
