using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Configuration;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Rules;

public sealed class NoTestChangesRule : IBlastGuardRule
{
  public string Id => "no-test-changes";

  public IEnumerable<RiskFinding> Analyse(
      PullRequestChangeSet changeSet,
      BlastGuardConfiguration configuration)
  {
    if (!RuleHelpers.IsRuleEnabled(configuration, Id))
    {
      yield break;
    }

    var files = RuleHelpers.GetRelevantFiles(changeSet, configuration).ToList();
    var nonTestNonDocFiles = files
        .Where(f => !RuleHelpers.IsTestFile(f.Path) && !RuleHelpers.IsDocumentationFile(f.Path))
        .ToList();

    if (nonTestNonDocFiles.Count == 0)
    {
      yield break;
    }

    var hasTestChanges = files.Any(f => RuleHelpers.IsTestFile(f.Path));
    if (!hasTestChanges)
    {
      yield return new RiskFinding(
          RiskCategory.Tests,
          Id,
          "No test files changed",
          "Production code changed but no test files were updated.",
          10);
    }
  }
}
