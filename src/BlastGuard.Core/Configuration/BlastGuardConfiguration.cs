namespace BlastGuard.Core.Configuration;

public sealed class BlastGuardConfiguration
{
    public RiskThresholdConfiguration Thresholds { get; init; } = RiskThresholdConfiguration.Default;

    public IReadOnlyList<string> IgnorePatterns { get; init; } = DefaultIgnorePatterns;

    public IReadOnlyList<string> BoundedAreaRoots { get; init; } = DefaultBoundedAreaRoots;

    public IReadOnlyList<string> SecurityPathHints { get; init; } = DefaultSecurityPathHints;

    public IReadOnlyDictionary<string, RuleConfiguration> Rules { get; init; } =
        new Dictionary<string, RuleConfiguration>();

    public static readonly IReadOnlyList<string> DefaultIgnorePatterns =
    [
        "**/*.Designer.cs",
        "**/*.g.cs",
        "**/bin/**",
        "**/obj/**",
        "**/Generated/**"
    ];

    public static readonly IReadOnlyList<string> DefaultBoundedAreaRoots =
    [
        "src/Features",
        "src/Modules",
        "src"
    ];

    public static readonly IReadOnlyList<string> DefaultSecurityPathHints =
    [
        "Auth",
        "Authentication",
        "Authorization",
        "Authorisation",
        "Claims"
    ];
}

public sealed class RiskThresholdConfiguration
{
    public int Medium { get; init; } = 25;

    public int High { get; init; } = 50;

    public int Critical { get; init; } = 75;

    public static RiskThresholdConfiguration Default { get; } = new();
}

public sealed class RuleConfiguration
{
    public bool Enabled { get; init; } = true;
}
