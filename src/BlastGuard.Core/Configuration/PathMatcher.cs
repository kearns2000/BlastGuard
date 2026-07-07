using Microsoft.Extensions.FileSystemGlobbing;

namespace BlastGuard.Core.Configuration;

public static class PathMatcher
{
    public static bool IsIgnored(string filePath, IReadOnlyList<string> ignorePatterns)
    {
        var normalised = NormalisePath(filePath);

        foreach (var pattern in ignorePatterns)
        {
            var matcher = new Matcher();
            matcher.AddInclude(pattern);
            if (matcher.Match(normalised).HasMatches)
            {
                return true;
            }
        }

        return false;
    }

    public static string NormalisePath(string path) =>
        path.Replace('\\', '/').TrimStart('/');
}
