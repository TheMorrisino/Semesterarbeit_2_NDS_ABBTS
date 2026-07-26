using System.Diagnostics;
using System.IO;
using System.Text.Json;

using Build.Tasks;

using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Solution;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace Build;

public static class Program
{
  public static int Main(string[] args)
  {
    return new CakeHost().UseContext<BuildContext>().Run(args);
  }
}

public class BuildContext : FrostingContext
{
  public DirectoryPath ArtifactsDir { get; }
  public DirectoryPath AppPublishDir { get; }
  public FilePath WebArchivePath { get; }
  public SolutionParserResult Solution { get; }

  public BuildContext(ICakeContext context)
    : base(context)
  {
    Solution = context.Argument(
      nameof(Solution),
      context.ParseSolution(context.File("../teaching.slnx"))
    );
    ArtifactsDir = context.Argument<DirectoryPath>(
      nameof(ArtifactsDir),
      context.Directory("../.artifacts")
    );
    AppPublishDir = context.Argument(nameof(AppPublishDir), ArtifactsDir.Combine("web"));

    string gitVersion = GetGitVersion();
    WebArchivePath = context.File(System.IO.Path.Combine(ArtifactsDir.FullPath, $"web-{gitVersion}.zip"));
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
[IsDependentOn(typeof(PackageWebArtifactsTask))]
public class DefaultTask : FrostingTask { }
