using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Configuration;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Rules;

public sealed class PublicContractRule : IBlastGuardRule
{
  private static readonly string[] ContractPathSegments =
  [
      "Contracts", "Contract", "Dtos", "Dto", "Requests", "Request",
      "Responses", "Response", "Endpoints", "Controllers", "OpenApi", "Swagger"
  ];

  private static readonly string[] RouteMappingIndicators =
  [
      "MapGet", "MapPost", "MapPut", "MapDelete",
      "HttpGet", "HttpPost", "HttpPut", "HttpDelete", "Route("
  ];

  public string Id => "public-contract";

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

      if (path.EndsWith(".proto", StringComparison.OrdinalIgnoreCase)
          || path.EndsWith(".graphql", StringComparison.OrdinalIgnoreCase))
      {
        yield return new RiskFinding(
            RiskCategory.PublicContract,
            Id,
            "Message contract file changed",
            $"Message contract file changed: {path}.",
            20,
            file.Path);
        continue;
      }

      if (RuleHelpers.PathContainsAny(path, "OpenApi", "Swagger")
          && (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
              || path.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase)
              || path.EndsWith(".yml", StringComparison.OrdinalIgnoreCase)))
      {
        yield return new RiskFinding(
            RiskCategory.PublicContract,
            Id,
            "OpenAPI or Swagger file changed",
            $"OpenAPI or Swagger file changed: {path}.",
            20,
            file.Path);
        continue;
      }

      if (RuleHelpers.PathContainsAny(path, ContractPathSegments))
      {
        yield return new RiskFinding(
            RiskCategory.PublicContract,
            Id,
            "Public contract file changed",
            $"Public contract file changed: {path}.",
            20,
            file.Path);
      }

      if (RuleHelpers.PatchContainsAny(file.Patch, RouteMappingIndicators))
      {
        yield return new RiskFinding(
            RiskCategory.PublicContract,
            Id,
            "Endpoint route mapping changed",
            $"Endpoint route mapping changed in: {path}.",
            15,
            file.Path);
      }
    }
  }
}
