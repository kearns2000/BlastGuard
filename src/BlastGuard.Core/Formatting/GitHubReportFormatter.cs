using System.Text;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Formatting;

public sealed class GitHubReportFormatter : IReportFormatter
{
    public string Format(BlastRadiusReport report, bool includeSuggestions = true)
    {
        var builder = new StringBuilder();
        builder.AppendLine("## BlastGuard report");
        builder.AppendLine();
        builder.AppendLine($"**Score:** {report.Score} / 100  ");
        builder.AppendLine($"**Risk:** {FormatRiskLevel(report.RiskLevel)}");
        builder.AppendLine();

        var findings = report.Findings.Where(f => f.Points != 0).ToList();
        if (findings.Count > 0)
        {
            builder.AppendLine("| Area | Finding | Points |");
            builder.AppendLine("|---|---|---:|");

            foreach (var finding in findings.Take(15))
            {
                var points = finding.Points > 0 ? $"+{finding.Points}" : finding.Points.ToString();
                builder.AppendLine($"| {FormatCategory(finding.Category)} | {finding.Title} | {points} |");
            }

            if (findings.Count > 15)
            {
                builder.AppendLine();
                builder.AppendLine($"_{findings.Count - 15} additional findings omitted._");
            }
        }

        if (includeSuggestions && report.SuggestedReviewFocus.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("### Suggested review focus");
            builder.AppendLine();
            foreach (var suggestion in report.SuggestedReviewFocus)
            {
                builder.AppendLine($"- {suggestion}");
            }
        }

        return builder.ToString().TrimEnd();
    }

    private static string FormatRiskLevel(RiskLevel level) => level switch
    {
        RiskLevel.Low => "Low",
        RiskLevel.Medium => "Medium",
        RiskLevel.High => "High",
        RiskLevel.Critical => "Critical",
        _ => level.ToString()
    };

    private static string FormatCategory(RiskCategory category) => category switch
    {
        RiskCategory.RuntimeBehaviour => "Runtime behaviour",
        RiskCategory.PublicContract => "Public contract",
        _ => category.ToString()
    };
}
