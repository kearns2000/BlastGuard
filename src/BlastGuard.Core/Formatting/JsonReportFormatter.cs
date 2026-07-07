using System.Text.Json;
using System.Text.Json.Serialization;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Formatting;

public sealed class JsonReportFormatter : IReportFormatter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public string Format(BlastRadiusReport report, bool includeSuggestions = true)
    {
        var dto = new
        {
            report.Score,
            risk = report.RiskLevel.ToString(),
            report.Summary,
            findings = report.Findings.Select(f => new
            {
                category = f.Category.ToString(),
                f.RuleId,
                f.Title,
                f.Message,
                f.Points,
                f.FilePath
            }),
            suggestedReviewFocus = includeSuggestions ? report.SuggestedReviewFocus : Array.Empty<string>()
        };

        return JsonSerializer.Serialize(dto, Options);
    }
}
