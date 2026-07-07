using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Configuration;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Rules;

public sealed class LargeChangeRule : IBlastGuardRule
{
  public string Id => "large-change";

  public IEnumerable<RiskFinding> Analyse(
      PullRequestChangeSet changeSet,
      BlastGuardConfiguration configuration)
  {
    if (!RuleHelpers.IsRuleEnabled(configuration, Id))
    {
      yield break;
    }

    var files = RuleHelpers.GetRelevantFiles(changeSet, configuration).ToList();
    var fileCount = files.Count;

    if (fileCount > 75)
    {
      yield return new RiskFinding(
          RiskCategory.Scope,
          Id,
          "Very large number of files changed",
          $"More than 75 files changed ({fileCount} files).",
          20);
    }
    else if (fileCount > 25)
    {
      yield return new RiskFinding(
          RiskCategory.Scope,
          Id,
          "Large number of files changed",
          $"More than 25 files changed ({fileCount} files).",
          10);
    }

    var totalLines = files
        .Select(f => (f.LinesAdded ?? 0) + (f.LinesDeleted ?? 0))
        .Sum();

    if (totalLines > 3000)
    {
      yield return new RiskFinding(
          RiskCategory.Scope,
          Id,
          "Very large line change",
          $"More than 3000 lines changed ({totalLines} lines).",
          30);
    }
    else if (totalLines > 1500)
    {
      yield return new RiskFinding(
          RiskCategory.Scope,
          Id,
          "Large line change",
          $"More than 1500 lines changed ({totalLines} lines).",
          20);
    }
    else if (totalLines > 500)
    {
      yield return new RiskFinding(
          RiskCategory.Scope,
          Id,
          "Moderate line change",
          $"More than 500 lines changed ({totalLines} lines).",
          10);
    }
  }
}
