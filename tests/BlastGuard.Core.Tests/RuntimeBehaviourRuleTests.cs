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

    [Fact]
    public void DetectsQueueHandlingChanges()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Messaging/OrderQueue.cs", patch: "public class OrderQueue { }"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.RuntimeBehaviour
            && f.Title.Contains("Queue"));
    }

    [Fact]
    public void DoesNotFlagDequeueAsQueueHandling()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Collections/StackHelpers.cs", patch: "var item = stack.Dequeue();"));

        Assert.DoesNotContain(report.Findings, f =>
            f.Category == Scoring.RiskCategory.RuntimeBehaviour
            && f.Title.Contains("Queue"));
    }
}
