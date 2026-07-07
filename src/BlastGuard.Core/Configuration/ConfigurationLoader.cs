using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlastGuard.Core.Configuration;

public static class ConfigurationLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static BlastGuardConfiguration Load(string? configPath, string repositoryPath)
    {
        var resolvedPath = ResolveConfigPath(configPath, repositoryPath);
        if (resolvedPath is null)
        {
            return new BlastGuardConfiguration();
        }

        var json = File.ReadAllText(resolvedPath);
        var loaded = JsonSerializer.Deserialize<BlastGuardConfigurationDto>(json, JsonOptions)
            ?? throw new InvalidOperationException($"BlastGuard could not read configuration file: {resolvedPath}");

        return loaded.ToConfiguration();
    }

    public static string? ResolveConfigPath(string? configPath, string repositoryPath)
    {
        if (!string.IsNullOrWhiteSpace(configPath))
        {
            return Path.GetFullPath(configPath);
        }

        var defaultPath = Path.Combine(Path.GetFullPath(repositoryPath), "blastguard.json");
        return File.Exists(defaultPath) ? defaultPath : null;
    }

    private sealed class BlastGuardConfigurationDto
    {
        public RiskThresholdConfiguration? Thresholds { get; init; }

        public List<string>? IgnorePatterns { get; init; }

        public List<string>? BoundedAreaRoots { get; init; }

        public List<string>? SecurityPathHints { get; init; }

        public Dictionary<string, RuleConfiguration>? Rules { get; init; }

        public BlastGuardConfiguration ToConfiguration() => new()
        {
            Thresholds = Thresholds ?? RiskThresholdConfiguration.Default,
            IgnorePatterns = IgnorePatterns ?? BlastGuardConfiguration.DefaultIgnorePatterns,
            BoundedAreaRoots = BoundedAreaRoots ?? BlastGuardConfiguration.DefaultBoundedAreaRoots,
            SecurityPathHints = SecurityPathHints ?? BlastGuardConfiguration.DefaultSecurityPathHints,
            Rules = Rules ?? new Dictionary<string, RuleConfiguration>()
        };
    }
}
