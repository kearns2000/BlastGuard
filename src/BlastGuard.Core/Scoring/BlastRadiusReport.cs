namespace BlastGuard.Core.Scoring;

public sealed record BlastRadiusReport(
    int Score,
    RiskLevel RiskLevel,
    IReadOnlyList<RiskFinding> Findings,
    IReadOnlyList<string> SuggestedReviewFocus,
    ReportSummary Summary);

public sealed record RiskFinding(
    RiskCategory Category,
    string RuleId,
    string Title,
    string Message,
    int Points,
    string? FilePath = null);

public sealed record ReportSummary(
    int TotalFilesChanged,
    int? TotalLinesAdded,
    int? TotalLinesDeleted,
    IReadOnlyList<string> MainRiskSignals);

public enum RiskCategory
{
    Scope,
    PublicContract,
    Database,
    Configuration,
    Security,
    RuntimeBehaviour,
    Tests,
    Dependencies,
    Infrastructure,
    Documentation,
    Unknown
}

public enum RiskLevel
{
    Low,
    Medium,
    High,
    Critical
}
