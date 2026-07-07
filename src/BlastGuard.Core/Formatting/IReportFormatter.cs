using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Formatting;

public interface IReportFormatter
{
    string Format(BlastRadiusReport report, bool includeSuggestions = true);
}
