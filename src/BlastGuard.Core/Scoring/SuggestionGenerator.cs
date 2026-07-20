namespace BlastGuard.Core.Scoring;

internal static class SuggestionGenerator
{
    private static readonly Dictionary<RiskCategory, string> CategorySuggestions = new()
    {
        [RiskCategory.PublicContract] =
            "Check whether API consumers, message consumers, or generated clients are affected.",
        [RiskCategory.Database] =
            "Confirm the migration is backwards compatible.",
        [RiskCategory.Configuration] =
            "Confirm the new configuration exists in every required environment.",
        [RiskCategory.Infrastructure] =
            "Confirm the new configuration exists in every required environment.",
        [RiskCategory.Security] =
            "Review authentication, authorisation, claims, and permission behaviour carefully.",
        [RiskCategory.RuntimeBehaviour] =
            "Check retry, timeout, cancellation, and failure behaviour.",
        [RiskCategory.Tests] =
            "Add or update tests around the highest-risk changed areas."
    };

    public static IReadOnlyList<string> Generate(IReadOnlyList<RiskFinding> findings)
    {
        var suggestions = new List<string>();

        foreach (var finding in findings.Where(f => f.Points > 0))
        {
            if (CategorySuggestions.TryGetValue(finding.Category, out var suggestion)
                && !suggestions.Contains(suggestion))
            {
                suggestions.Add(suggestion);
            }

            if (finding.Category == RiskCategory.Database
                && finding.Title.Contains("destructive", StringComparison.OrdinalIgnoreCase))
            {
                const string dataSafety = "Check whether the change is safe for existing production data.";
                if (!suggestions.Contains(dataSafety))
                {
                    suggestions.Add(dataSafety);
                }
            }

            if (finding.Category == RiskCategory.Security
                && (finding.Title.Contains("Claims", StringComparison.OrdinalIgnoreCase)
                    || finding.Title.Contains("Authorisation", StringComparison.OrdinalIgnoreCase)
                    || finding.Title.Contains("Authentication", StringComparison.OrdinalIgnoreCase)))
            {
                const string permissions =
                    "Check whether the change affects permissions, claims, or token validation.";
                if (!suggestions.Contains(permissions))
                {
                    suggestions.Add(permissions);
                }
            }
        }

        if (findings.Any(f => f.RuleId == "no-test-changes"))
        {
            const string testSuggestion = "Add or update tests around the highest-risk changed areas.";
            if (!suggestions.Contains(testSuggestion))
            {
                suggestions.Add(testSuggestion);
            }
        }

        return suggestions;
    }
}
