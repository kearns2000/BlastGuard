using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Configuration;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Rules;

public static class RuleHelpers
{
    public static bool IsRuleEnabled(BlastGuardConfiguration configuration, string ruleId)
    {
        if (configuration.Rules.TryGetValue(ruleId, out var ruleConfig))
        {
            return ruleConfig.Enabled;
        }

        return true;
    }

    public static IEnumerable<ChangedFile> GetRelevantFiles(
        PullRequestChangeSet changeSet,
        BlastGuardConfiguration configuration) =>
        changeSet.Files.Where(f => !PathMatcher.IsIgnored(f.Path, configuration.IgnorePatterns));

    public static bool PathContainsAny(string path, params string[] segments) =>
        segments.Any(segment => path.Contains(segment, StringComparison.OrdinalIgnoreCase));

    public static bool PathContainsSegment(string path, string segment)
    {
        var normalised = PathMatcher.NormalisePath(path);
        return normalised
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Any(part => part.Equals(segment, StringComparison.OrdinalIgnoreCase));
    }

    public static bool PatchContainsAny(string? patch, params string[] indicators) =>
        patch is not null && indicators.Any(indicator =>
            patch.Contains(indicator, StringComparison.Ordinal));

    public static bool PathEndsWithAny(string path, params string[] suffixes) =>
        suffixes.Any(suffix => path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

    public static bool IsDocumentationFile(string path)
    {
        var normalised = PathMatcher.NormalisePath(path);
        if (normalised.Equals("README.md", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (normalised.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (normalised.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return normalised.StartsWith("docs/", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsTestFile(string path)
    {
        var normalised = PathMatcher.NormalisePath(path);
        return normalised.Contains(".Tests", StringComparison.OrdinalIgnoreCase)
            || normalised.Contains("/Tests/", StringComparison.OrdinalIgnoreCase)
            || normalised.Contains("/Test/", StringComparison.OrdinalIgnoreCase)
            || normalised.EndsWith("Tests.cs", StringComparison.OrdinalIgnoreCase)
            || normalised.EndsWith("Test.cs", StringComparison.OrdinalIgnoreCase);
    }
}
