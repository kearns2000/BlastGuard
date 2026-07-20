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
            || fileName.Equals("appsettings.Prod.json", StringComparison.OrdinalIgnoreCase)
            || fileName.Contains(".Prod.", StringComparison.OrdinalIgnoreCase);

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

  private static bool IsInfrastructureConfig(string path)
  {
    var normalised = PathMatcher.NormalisePath(path);

    // Workflow and GitHub metadata changes are not infrastructure configuration.
    if (normalised.StartsWith(".github/", StringComparison.OrdinalIgnoreCase))
    {
      return false;
    }

    if (normalised.EndsWith(".bicep", StringComparison.OrdinalIgnoreCase)
        || normalised.EndsWith(".tf", StringComparison.OrdinalIgnoreCase)
        || normalised.EndsWith("/Dockerfile", StringComparison.OrdinalIgnoreCase)
        || normalised.Equals("Dockerfile", StringComparison.OrdinalIgnoreCase)
        || Path.GetFileName(normalised).StartsWith("docker-compose", StringComparison.OrdinalIgnoreCase))
    {
      return true;
    }

    if (!normalised.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase)
        && !normalised.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
    {
      return false;
    }

    var fileName = Path.GetFileName(normalised);
    if (fileName.StartsWith("docker-compose", StringComparison.OrdinalIgnoreCase)
        || fileName.Equals("compose.yaml", StringComparison.OrdinalIgnoreCase)
        || fileName.Equals("compose.yml", StringComparison.OrdinalIgnoreCase)
        || fileName.Equals("values.yaml", StringComparison.OrdinalIgnoreCase)
        || fileName.Equals("values.yml", StringComparison.OrdinalIgnoreCase)
        || fileName.Equals("Chart.yaml", StringComparison.OrdinalIgnoreCase)
        || fileName.Equals("Chart.yml", StringComparison.OrdinalIgnoreCase))
    {
      return true;
    }

    var segments = normalised.Split('/', StringSplitOptions.RemoveEmptyEntries);
    return segments.Any(segment =>
        segment.Equals("infra", StringComparison.OrdinalIgnoreCase)
        || segment.Equals("infrastructure", StringComparison.OrdinalIgnoreCase)
        || segment.Equals("deploy", StringComparison.OrdinalIgnoreCase)
        || segment.Equals("deployment", StringComparison.OrdinalIgnoreCase)
        || segment.Equals("deployments", StringComparison.OrdinalIgnoreCase)
        || segment.Equals("k8s", StringComparison.OrdinalIgnoreCase)
        || segment.Equals("kubernetes", StringComparison.OrdinalIgnoreCase)
        || segment.Equals("helm", StringComparison.OrdinalIgnoreCase)
        || segment.Equals("charts", StringComparison.OrdinalIgnoreCase)
        || segment.Equals("terraform", StringComparison.OrdinalIgnoreCase)
        || segment.Equals("pulumi", StringComparison.OrdinalIgnoreCase)
        || segment.Equals("ops", StringComparison.OrdinalIgnoreCase));
  }
}
