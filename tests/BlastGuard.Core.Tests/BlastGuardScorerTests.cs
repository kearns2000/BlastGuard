using BlastGuard.Core.Configuration;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Tests;

public class BlastGuardScorerTests
{
    [Fact]
    public void Score_IsClampedBetweenZeroAndOneHundred()
    {
        var manyRiskyFiles = Enumerable.Range(0, 100)
            .Select(i => TestFixtures.File(
                $"src/Features/Area{i}/Controllers/ApiController.cs",
                patch: "MapGet(\"/api/test\", () => {}); AddAuthentication(); migrationBuilder.DropTable(\"Users\");",
                linesAdded: 500,
                linesDeleted: 500))
            .ToArray();

        var report = TestFixtures.Score(manyRiskyFiles);

        Assert.InRange(report.Score, 0, 100);
    }

    [Fact]
    public void RiskLevel_UsesConfigurableThresholds()
    {
        var configuration = new BlastGuardConfiguration
        {
            Thresholds = new RiskThresholdConfiguration
            {
                Medium = 10,
                High = 20,
                Critical = 30
            }
        };

        var report = BlastGuardScorer.CreateDefault().Score(
            TestFixtures.CreateChangeSet(TestFixtures.File("docs/overview.md")),
            configuration);

        Assert.Equal(0, report.Score);
        Assert.Equal(RiskLevel.Low, BlastGuardScorer.DetermineRiskLevel(report.Score, configuration.Thresholds));

        var riskyReport = BlastGuardScorer.CreateDefault().Score(
            TestFixtures.CreateChangeSet(
                TestFixtures.File("src/Api/appsettings.json"),
                TestFixtures.File("tests/Api/ApiConfigTests.cs")),
            configuration);

        Assert.Equal(10, riskyReport.Score);
        Assert.Equal(RiskLevel.Medium, riskyReport.RiskLevel);
    }

    [Theory]
    [InlineData(0, RiskLevel.Low)]
    [InlineData(24, RiskLevel.Low)]
    [InlineData(25, RiskLevel.Medium)]
    [InlineData(49, RiskLevel.Medium)]
    [InlineData(50, RiskLevel.High)]
    [InlineData(74, RiskLevel.High)]
    [InlineData(75, RiskLevel.Critical)]
    [InlineData(100, RiskLevel.Critical)]
    public void DetermineRiskLevel_UsesDefaultThresholds(int score, RiskLevel expected)
    {
        var level = BlastGuardScorer.DetermineRiskLevel(score, RiskThresholdConfiguration.Default);
        Assert.Equal(expected, level);
    }
}
