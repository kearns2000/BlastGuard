using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Configuration;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Rules;

public sealed class DocumentationOnlyRule : IBlastGuardRule
{
  public string Id => "documentation-only";

  public IEnumerable<RiskFinding> Analyse(
      PullRequestChangeSet changeSet,
      BlastGuardConfiguration configuration)
  {
    if (!RuleHelpers.IsRuleEnabled(configuration, Id))
    {
      yield break;
    }

    var files = RuleHelpers.GetRelevantFiles(changeSet, configuration).ToList();
    if (files.Count == 0)
    {
      yield break;
    }

    if (files.All(f => RuleHelpers.IsDocumentationFile(f.Path)))
    {
      yield return new RiskFinding(
          RiskCategory.Documentation,
          Id,
          "Documentation-only change",
          "All changed files are documentation files.",
          -30);
    }
  }
}
