using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Configuration;
using BlastGuard.Core.Rules;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Scoring;

public sealed class BlastGuardScorer(IEnumerable<IBlastGuardRule> rules)
{
    public static BlastGuardScorer CreateDefault() => new(
    [
        new BoundedAreaRule(),
        new PublicContractRule(),
        new DatabaseRule(),
        new ConfigurationRule(),
        new SecuritySensitiveRule(),
        new RuntimeBehaviourRule(),
        new TestSignalRule(),
        new NoTestChangesRule(),
        new DependencyRule(),
        new LargeChangeRule(),
        new DocumentationOnlyRule()
    ]);

    public BlastRadiusReport Score(
        PullRequestChangeSet changeSet,
        BlastGuardConfiguration configuration)
    {
        var findings = rules
            .SelectMany(rule => rule.Analyse(changeSet, configuration))
            .ToList();

        var rawScore = findings.Sum(f => f.Points);
        var score = Math.Clamp(rawScore, 0, 100);
        var riskLevel = DetermineRiskLevel(score, configuration.Thresholds);
        var suggestions = SuggestionGenerator.Generate(findings);
        var summary = BuildSummary(changeSet, configuration, findings);

        return new BlastRadiusReport(score, riskLevel, findings, suggestions, summary);
    }

    internal static RiskLevel DetermineRiskLevel(int score, RiskThresholdConfiguration thresholds)
    {
        if (score >= thresholds.Critical)
        {
            return RiskLevel.Critical;
        }

        if (score >= thresholds.High)
        {
            return RiskLevel.High;
        }

        if (score >= thresholds.Medium)
        {
            return RiskLevel.Medium;
        }

        return RiskLevel.Low;
    }

    private static ReportSummary BuildSummary(
        PullRequestChangeSet changeSet,
        BlastGuardConfiguration configuration,
        IReadOnlyList<RiskFinding> findings)
    {
        var relevantFiles = RuleHelpers.GetRelevantFiles(changeSet, configuration).ToList();

        var mainSignals = findings
            .Where(f => f.Points > 0)
            .OrderByDescending(f => f.Points)
            .ThenBy(f => f.Category)
            .Select(f => f.Title)
            .Distinct()
            .Take(5)
            .ToList();

        return new ReportSummary(
            relevantFiles.Count,
            relevantFiles.Sum(f => f.LinesAdded),
            relevantFiles.Sum(f => f.LinesDeleted),
            mainSignals);
    }
}
