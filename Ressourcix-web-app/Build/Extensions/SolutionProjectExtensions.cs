using Cake.Common.Solution;
using Cake.Common.Tools.Command;

namespace Build.Extensions;

public static class SolutionProjectExtensions
{
  public static CommandSettings GetPnpmCommand(this SolutionProject solutionProject)
  {
    return new CommandSettings
    {
      ToolName = "pnpm",
      ToolExecutableNames = ["pnpm", "pnpm.cmd", "pnpm.bat", "pnpm.ps1", "pnpm.exe"],
      WorkingDirectory = solutionProject.Path.GetDirectory(),
    };
  }
}