using BlastGuard.Core.Formatting;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Formatting;

public static class ReportFormatterFactory
{
    public static IReportFormatter Create(string format) => format.ToLowerInvariant() switch
    {
        "text" => new TextReportFormatter(),
        "json" => new JsonReportFormatter(),
        "markdown" => new MarkdownReportFormatter(),
        "github" => new GitHubReportFormatter(),
        _ => throw new ArgumentException(
            $"Unknown format: {format}. Supported formats are text, json, markdown, github.")
    };

    public static IReadOnlyList<string> SupportedFormats { get; } =
        ["text", "json", "markdown", "github"];
}
