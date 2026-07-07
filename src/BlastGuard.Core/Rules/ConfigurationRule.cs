using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Configuration;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Rules;

public sealed class ConfigurationRule : IBlastGuardRule
{
  public string Id => "configuration";

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

      if (IsInfrastructureConfig(path))
      {
        yield return new RiskFinding(
            RiskCategory.Infrastructure,
            Id,
            "Infrastructure configuration changed",
            $"Infrastructure configuration changed: {path}.",
            15,
            file.Path);
        continue;
      }

      if (fileName.Equals("appsettings.json", StringComparison.OrdinalIgnoreCase)
          || (fileName.StartsWith("appsettings.", StringComparison.OrdinalIgnoreCase)
              && fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
      {
        var isProduction = fileName.Contains("Production", StringComparison.OrdinalIgnoreCase)
            || fileName.Contains("Prod", StringComparison.OrdinalIgnoreCase);

        yield return new RiskFinding(
            RiskCategory.Configuration,
            Id,
            isProduction ? "Production appsettings changed" : "Appsettings changed",
            isProduction
                ? $"Production appsettings changed: {path}."
                : $"General appsettings changed: {path}.",
            isProduction ? 15 : 10,
            file.Path);
        continue;
      }

      if (fileName.Equals("launchSettings.json", StringComparison.OrdinalIgnoreCase))
      {
        yield return new RiskFinding(
            RiskCategory.Configuration,
            Id,
            "Launch settings changed",
            $"Launch settings changed: {path}.",
            10,
            file.Path);
        continue;
      }

      if (IsOptionsClassFile(path))
      {
        yield return new RiskFinding(
            RiskCategory.Configuration,
            Id,
            "Options class changed",
            $"Options class changed: {path}.",
            10,
            file.Path);
      }
    }
  }

  internal static bool IsOptionsClassFile(string path)
  {
    if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
    {
      return false;
    }

    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);

    if (fileNameWithoutExtension.EndsWith("Options", StringComparison.OrdinalIgnoreCase)
        || fileNameWithoutExtension.EndsWith("Settings", StringComparison.OrdinalIgnoreCase)
        || fileNameWithoutExtension.EndsWith("Configuration", StringComparison.OrdinalIgnoreCase))
    {
      return true;
    }

    var segments = PathMatcher.NormalisePath(path).Split('/');
    return segments.Any(segment =>
        segment.Equals("Options", StringComparison.OrdinalIgnoreCase)
        || segment.Equals("Settings", StringComparison.OrdinalIgnoreCase)
        || segment.Equals("Configuration", StringComparison.OrdinalIgnoreCase));
  }

  private static bool IsInfrastructureConfig(string path) =>
      path.EndsWith(".bicep", StringComparison.OrdinalIgnoreCase)
      || path.EndsWith(".tf", StringComparison.OrdinalIgnoreCase)
      || path.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase)
      || path.EndsWith(".yml", StringComparison.OrdinalIgnoreCase)
      || path.EndsWith("Dockerfile", StringComparison.OrdinalIgnoreCase)
      || path.EndsWith("docker-compose.yml", StringComparison.OrdinalIgnoreCase)
      || path.EndsWith("docker-compose.yaml", StringComparison.OrdinalIgnoreCase);
}
