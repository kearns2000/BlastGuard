using BlastGuard.Core.Rules;

namespace BlastGuard.Core.Tests;

public class TestSignalRuleTests
{
    [Fact]
    public void AppliesNegativeScoreWhenOnlyTestsChanged()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("tests/BlastGuard.Core.Tests/ExampleTests.cs"));

        Assert.Contains(report.Findings, f => f.Points == -20 && f.Title.Contains("Only tests"));
    }

    [Fact]
    public void AddsPenaltyWhenProductionCodeChangedWithoutTests()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Features/Payments/PaymentService.cs"));

        Assert.Contains(report.Findings, f => f.Points == 10 && f.Title.Contains("No test files"));
    }

    [Fact]
    public void AppliesMatchingTestAreaReduction()
    {
        var files = new[]
        {
            TestFixtures.File("src/Features/Payments/PaymentService.cs"),
            TestFixtures.File("tests/Features/Payments/PaymentServiceTests.cs")
        };

        var report = TestFixtures.Score(files);

        Assert.Contains(report.Findings, f => f.Points == -5 && f.Title.Contains("Matching test area"));
        Assert.True(TestSignalRule.HasMatchingTestArea(
            files.Where(f => !f.Path.Contains("Tests")).ToList(),
            files.Where(f => f.Path.Contains("Tests")).ToList()));
    }
}
