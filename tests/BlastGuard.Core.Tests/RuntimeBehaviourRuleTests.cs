namespace BlastGuard.Core.Tests;

public class RuntimeBehaviourRuleTests
{
    [Fact]
    public void DetectsBackgroundWorkerChanges()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Workers/OrderProcessor.cs", patch: "public class OrderProcessor : BackgroundService"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.RuntimeBehaviour
            && f.Title.Contains("Background worker"));
    }

    [Fact]
    public void DetectsRetryBehaviourChanges()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Infrastructure/Resilience/RetryPolicies.cs", patch: "Policy.Handle<Exception>().Retry(3);"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.RuntimeBehaviour
            && f.Title.Contains("Retry"));
    }
}
