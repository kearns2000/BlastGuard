using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Configuration;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Rules;

public sealed class BoundedAreaRule : IBlastGuardRule
{
    public string Id => "bounded-area";

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

        var areas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            var area = InferArea(file.Path, configuration.BoundedAreaRoots);
            if (area is not null)
            {
                areas.Add(area);
            }
        }

        if (areas.Count == 0)
        {
            yield break;
        }

        var points = areas.Count switch
        {
            1 => 0,
            2 => 5,
            3 => 10,
            4 => 15,
            _ => 25
        };

        if (points == 0)
        {
            yield break;
        }

        var areaList = string.Join(", ", areas.OrderBy(a => a));
        yield return new RiskFinding(
            RiskCategory.Scope,
            Id,
            "Multiple bounded areas touched",
            $"This PR touches {areas.Count} bounded areas: {areaList}.",
            points);
    }

    internal static string? InferArea(string filePath, IReadOnlyList<string> roots)
    {
        var normalised = PathMatcher.NormalisePath(filePath);

        foreach (var root in roots.OrderByDescending(r => r.Length))
        {
            var normalisedRoot = PathMatcher.NormalisePath(root).TrimEnd('/');
            if (!normalised.StartsWith(normalisedRoot + "/", StringComparison.OrdinalIgnoreCase)
                && !normalised.Equals(normalisedRoot, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var remainder = normalised.Length > normalisedRoot.Length
                ? normalised[(normalisedRoot.Length + 1)..]
                : string.Empty;

            if (string.IsNullOrEmpty(remainder))
            {
                return normalisedRoot;
            }

            var segment = remainder.Split('/')[0];
            return $"{normalisedRoot}/{segment}";
        }

        return null;
    }
}
