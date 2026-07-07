using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Configuration;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Rules;

public sealed class DependencyRule : IBlastGuardRule
{
  private static readonly string[] PatchIndicators =
  [
      "PackageReference", "ProjectReference", "Version=", "TargetFramework", "TargetFrameworks"
  ];

  public string Id => "dependency";

  public IEnumerable<RiskFinding> Analyse(
      PullRequestChangeSet changeSet,
      BlastGuardConfiguration configuration)
  {
    if (!RuleHelpers.IsRuleEnabled(configuration, Id))
    {
      yield break;
    }

    foreach (var file in RuleHelpers.GetRelevantFiles(changeSet, configuration))
    {
      var path = PathMatcher.NormalisePath(file.Path);
      var fileName = Path.GetFileName(path);

      if (fileName.Equals("global.json", StringComparison.OrdinalIgnoreCase))
      {
        yield return new RiskFinding(
            RiskCategory.Dependencies,
            Id,
            "global.json changed",
            $"global.json changed: {path}.",
            15,
            file.Path);
        continue;
      }

      if (fileName.Equals("NuGet.config", StringComparison.OrdinalIgnoreCase)
          || fileName.Equals("nuget.config", StringComparison.OrdinalIgnoreCase))
      {
        yield return new RiskFinding(
            RiskCategory.Dependencies,
            Id,
            "NuGet.config changed",
            $"NuGet.config changed: {path}.",
            15,
            file.Path);
        continue;
      }

      if (fileName.Equals("Directory.Packages.props", StringComparison.OrdinalIgnoreCase)
          || fileName.Equals("Directory.Build.props", StringComparison.OrdinalIgnoreCase)
          || fileName.Equals("packages.lock.json", StringComparison.OrdinalIgnoreCase))
      {
        yield return new RiskFinding(
            RiskCategory.Dependencies,
            Id,
            "Dependency manifest changed",
            $"Dependency manifest changed: {path}.",
            10,
            file.Path);
        continue;
      }

      if (!path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
      {
        continue;
      }

      var patch = file.Patch ?? string.Empty;

      if (patch.Contains("TargetFramework", StringComparison.Ordinal)
          || patch.Contains("TargetFrameworks", StringComparison.Ordinal))
      {
        yield return new RiskFinding(
            RiskCategory.Dependencies,
            Id,
            "Target framework changed",
            $"Target framework changed in: {path}.",
            20,
            file.Path);
      }

      if (patch.Contains("PackageReference", StringComparison.Ordinal))
      {
        yield return new RiskFinding(
            RiskCategory.Dependencies,
            Id,
            "Package reference changed",
            $"Package reference changed in: {path}.",
            10,
            file.Path);
      }

      if (patch.Contains("ProjectReference", StringComparison.Ordinal))
      {
        yield return new RiskFinding(
            RiskCategory.Dependencies,
            Id,
            "Project reference changed",
            $"Project reference changed in: {path}.",
            10,
            file.Path);
      }
    }
  }
}
