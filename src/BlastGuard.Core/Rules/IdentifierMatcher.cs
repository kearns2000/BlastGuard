using System.Text.RegularExpressions;

namespace BlastGuard.Core.Rules;

/// <summary>
/// Matches indicators against code and paths using identifier-aware tokenisation.
/// This avoids the common false positive where a short indicator such as "Auth"
/// matches an unrelated longer word such as "Author".
/// </summary>
public static partial class IdentifierMatcher
{
    [GeneratedRegex("[A-Z]+(?![a-z])|[A-Z][a-z]*|[a-z]+|[0-9]+", RegexOptions.CultureInvariant)]
    private static partial Regex WordRegex();

    public static bool ContainsIdentifier(string? text, string indicator)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        var indicatorWords = Tokenise(indicator);
        if (indicatorWords.Count == 0)
        {
            return false;
        }

        foreach (var line in text.Split('\n'))
        {
            var words = Tokenise(line);
            if (ContainsSubsequence(words, indicatorWords))
            {
                return true;
            }
        }

        return false;
    }

    public static bool ContainsAnyIdentifier(string? text, IEnumerable<string> indicators) =>
        indicators.Any(indicator => ContainsIdentifier(text, indicator));

    internal static List<string> Tokenise(string value)
    {
        var words = new List<string>();
        foreach (Match match in WordRegex().Matches(value))
        {
            words.Add(match.Value.ToLowerInvariant());
        }

        return words;
    }

    private static bool ContainsSubsequence(List<string> haystack, List<string> needle)
    {
        if (needle.Count == 0 || haystack.Count < needle.Count)
        {
            return false;
        }

        for (var start = 0; start <= haystack.Count - needle.Count; start++)
        {
            var matched = true;
            for (var offset = 0; offset < needle.Count; offset++)
            {
                if (!haystack[start + offset].Equals(needle[offset], StringComparison.Ordinal))
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return true;
            }
        }

        return false;
    }
}
