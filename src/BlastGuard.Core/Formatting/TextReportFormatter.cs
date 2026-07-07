using System.Text;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Formatting;

public sealed class TextReportFormatter : IReportFormatter
{
    public string Format(BlastRadiusReport report, bool includeSuggestions = true)
    {
        var builder = new StringBuilder();
        builder.AppendLine("BlastGuard report");
        builder.AppendLine();
        builder.AppendLine($"Score: {report.Score} / 100");
        builder.AppendLine($"Risk: {FormatRiskLevel(report.RiskLevel)}");
        builder.AppendLine();

        var scoredFindings = report.Findings.Where(f => f.Points != 0).ToList();
        var riskFindings = scoredFindings.Where(f => f.Points > 0).ToList();
        var mitigatingFindings = scoredFindings.Where(f => f.Points < 0).ToList();

        builder.AppendLine("Main risk signals:");
        if (riskFindings.Count > 0)
        {
            foreach (var signal in report.Summary.MainRiskSignals)
            {
                builder.AppendLine($"- {signal}");
            }

            var otherFindings = riskFindings
                .Where(f => !report.Summary.MainRiskSignals.Contains(f.Title))
                .Select(f => f.Title)
                .Distinct()
                .ToList();

            foreach (var title in otherFindings)
            {
                builder.AppendLine($"- {title}");
            }
        }
        else
        {
            builder.AppendLine("- No significant risk signals detected.");
        }

        if (mitigatingFindings.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Mitigating signals:");
            foreach (var finding in mitigatingFindings.Select(f => f.Title).Distinct())
            {
                builder.AppendLine($"- {finding}");
            }
        }

        if (includeSuggestions && report.SuggestedReviewFocus.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Suggested review focus:");
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
}
