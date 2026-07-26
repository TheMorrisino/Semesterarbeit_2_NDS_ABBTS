using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Solution;
using Cake.Common.Tools.Command;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace Build.Tasks;

public class BuildTeachingNdsWegTask : FrostingTask<BuildContext>
{
  private sealed record ManifestYear(int Year, List<int> Chapters);

  private sealed record ManifestRoot(string CoursePath, List<ManifestYear> Years);

  public override void Run(BuildContext context)
  {
    context.Information("*** Building NDS Web Engineering slides...");
    SolutionProject slidesProject =
      context.Solution.Projects.FirstOrDefault(project => project.Name == "Teaching.Slides")
      ?? throw new CakeException("Teaching.Slides project not found in the solution.");

    DirectoryPath slidesProjectDir = slidesProject
      .Path.GetDirectory()
      .Combine("../Teaching.Slides");
    CommandSettings pnpmCommand = new CommandSettings
    {
      ToolName = "pnpm",
      ToolExecutableNames = ["pnpm", "pnpm.cmd", "pnpm.bat", "pnpm.ps1", "pnpm.exe"],
      WorkingDirectory = slidesProjectDir,
    };

    Dictionary<int, HashSet<int>> discoveredChaptersByYear = [];

    FilePathCollection slideFiles = context.GetFiles($"{slidesProjectDir}/slides-*-chapter-*.md");
    IEnumerable<IGrouping<string, FilePath>> slideFilesByYear = slideFiles
      .GroupBy(filePath => ExtractYearInfix(filePath.GetFilenameWithoutExtension().ToString()))
      .OrderByDescending(grouping => grouping.Key);

    foreach (IGrouping<string, FilePath> yearlySlideFiles in slideFilesByYear)
    {
      ProcessSlideFiles(
        context,
        pnpmCommand,
        slidesProjectDir,
        yearlySlideFiles,
        yearlySlideFiles.Key,
        discoveredChaptersByYear
      );
    }

    WriteManifest(context, discoveredChaptersByYear);

    context.CopyDirectory(slidesProjectDir.Combine("public"), context.AppPublishDir);
  }

  private void ProcessSlideFiles(
    BuildContext context,
    CommandSettings pnpmCommand,
    DirectoryPath projectDir,
    IEnumerable<FilePath> slideFiles,
    string yearInfix,
    Dictionary<int, HashSet<int>> discoveredChaptersByYear
  )
  {
    int year = int.Parse(yearInfix);

    foreach (FilePath slideFile in slideFiles)
    {
      context.Information($"*** Processing {slideFile.FullPath}...");
      string filename = slideFile.GetFilenameWithoutExtension().ToString();

      // Extract chapter name: slides-<year>-chapter-1 -> chapter-1
      string chapterName = filename.Replace($"slides-{yearInfix}-", string.Empty);
      int chapterNumber = ParseChapterNumber(chapterName);
      TrackDiscoveredChapter(discoveredChaptersByYear, year, chapterNumber);

      context.Command(pnpmCommand, $"run build-{yearInfix}-{chapterName}");
      DirectoryPath distDir = projectDir.Combine("dist");
      DirectoryPath outputDir = context
        .AppPublishDir.Combine("nds-web-engineering")
        .Combine(yearInfix)
        .Combine(chapterName)
        .Combine("slidev");
      context.EnsureDirectoryDoesNotExist(outputDir);
      context.EnsureDirectoryExists(outputDir);
      context.CopyDirectory(distDir, outputDir);
    }
  }

  private static string ExtractYearInfix(string filename)
  {
    if (!filename.StartsWith("slides-", StringComparison.Ordinal))
    {
      throw new CakeException($"Unsupported slide filename format '{filename}'.");
    }

    int yearStart = "slides-".Length;
    int yearEnd = filename.IndexOf("-chapter-", StringComparison.Ordinal);
    if (yearEnd <= yearStart)
    {
      throw new CakeException($"Unsupported slide filename format '{filename}'.");
    }

    return filename[yearStart..yearEnd];
  }

  private static int ParseChapterNumber(string chapterName)
  {
    if (!chapterName.StartsWith("chapter-"))
    {
      throw new CakeException($"Unsupported chapter naming format '{chapterName}'.");
    }

    string suffix = chapterName["chapter-".Length..];
    if (!int.TryParse(suffix, out int chapterNumber))
    {
      throw new CakeException($"Could not parse chapter number from '{chapterName}'.");
    }

    return chapterNumber;
  }

  private static void TrackDiscoveredChapter(
    Dictionary<int, HashSet<int>> discoveredChaptersByYear,
    int year,
    int chapterNumber
  )
  {
    if (!discoveredChaptersByYear.TryGetValue(year, out HashSet<int> chapters))
    {
      chapters = [];
      discoveredChaptersByYear[year] = chapters;
    }

    chapters.Add(chapterNumber);
  }

  private static void WriteManifest(
    BuildContext context,
    Dictionary<int, HashSet<int>> discoveredChaptersByYear
  )
  {
    List<ManifestYear> years = discoveredChaptersByYear
      .OrderByDescending(entry => entry.Key)
      .Select(entry => new ManifestYear(entry.Key, [.. entry.Value.OrderByDescending(chapter => chapter)]))
      .ToList();

    ManifestRoot manifest = new("/nds-web-engineering", years);
    string manifestJson = JsonSerializer.Serialize(
      manifest,
      new JsonSerializerOptions { WriteIndented = true }
    );

    DirectoryPath manifestDirectory = context.AppPublishDir.Combine("nds-web-engineering");
    context.EnsureDirectoryExists(manifestDirectory);

    FilePath manifestFile = manifestDirectory.CombineWithFilePath("manifest.json");
    context.Information($"*** Writing course manifest to {manifestFile.FullPath}...");
    File.WriteAllText(manifestFile.FullPath, manifestJson);
  }
}
