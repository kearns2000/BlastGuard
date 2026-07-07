using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Configuration;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Rules;

public sealed class TestSignalRule : IBlastGuardRule
{
  public string Id => "test-signal";

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

    var testFiles = files.Where(f => RuleHelpers.IsTestFile(f.Path)).ToList();
    var nonTestFiles = files.Where(f => !RuleHelpers.IsTestFile(f.Path)).ToList();
    var nonDocNonTestFiles = nonTestFiles.Where(f => !RuleHelpers.IsDocumentationFile(f.Path)).ToList();

    if (nonTestFiles.Count == 0 && testFiles.Count > 0)
    {
      yield return new RiskFinding(
          RiskCategory.Tests,
          Id,
          "Only tests changed",
          "Only test files were changed in this pull request.",
          -20);
      yield break;
    }

    if (HasMatchingTestArea(nonTestFiles, testFiles))
    {
      yield return new RiskFinding(
          RiskCategory.Tests,
          Id,
          "Matching test area changed",
          "Tests in a matching area were updated alongside production code changes.",
          -5);
    }
  }

  internal static bool HasMatchingTestArea(
      IReadOnlyList<ChangedFile> nonTestFiles,
      IReadOnlyList<ChangedFile> testFiles)
  {
    foreach (var prodFile in nonTestFiles)
    {
      var areaToken = ExtractAreaToken(prodFile.Path);
      if (areaToken is null)
      {
        continue;
      }

      if (testFiles.Any(t => t.Path.Contains(areaToken, StringComparison.OrdinalIgnoreCase)))
      {
        return true;
      }
    }

    return false;
  }

  private static string? ExtractAreaToken(string path)
  {
    var normalised = PathMatcher.NormalisePath(path);
    var segments = normalised.Split('/');

    for (var i = 0; i < segments.Length - 1; i++)
    {
      if (segments[i].Equals("Features", StringComparison.OrdinalIgnoreCase)
          || segments[i].Equals("Modules", StringComparison.OrdinalIgnoreCase))
      {
        return segments[i + 1];
      }
    }

    return null;
  }
}
