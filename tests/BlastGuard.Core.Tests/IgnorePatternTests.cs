using BlastGuard.Core.Configuration;

namespace BlastGuard.Core.Tests;

public class IgnorePatternTests
{
    [Fact]
    public void IgnoresConfiguredGeneratedFiles()
    {
        var configuration = new BlastGuardConfiguration
        {
            IgnorePatterns = ["**/*.g.cs", "**/Generated/**"]
        };

        var changeSet = TestFixtures.CreateChangeSet(
            TestFixtures.File("src/Generated/Auto.g.cs", linesAdded: 5000, linesDeleted: 0),
            TestFixtures.File("src/Features/Payments/PaymentService.cs"));

        var report = Scoring.BlastGuardScorer.CreateDefault().Score(changeSet, configuration);

        Assert.DoesNotContain(report.Findings, f => f.Message.Contains("3000 lines"));
        Assert.Contains(report.Findings, f => f.Title.Contains("No test files"));
    }
}
