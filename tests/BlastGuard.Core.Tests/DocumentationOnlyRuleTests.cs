namespace BlastGuard.Core.Tests;

public class DocumentationOnlyRuleTests
{
    [Fact]
    public void ReducesScoreForDocumentationOnlyChanges()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("README.md"),
            TestFixtures.File("docs/architecture.md"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.Documentation
            && f.Points == -30);
    }
}
