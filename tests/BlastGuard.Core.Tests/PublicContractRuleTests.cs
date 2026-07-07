namespace BlastGuard.Core.Tests;

public class PublicContractRuleTests
{
    [Fact]
    public void DetectsPublicContractFileChanges()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Contracts/OrderResponse.cs"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.PublicContract
            && f.Points == 20
            && f.Title.Contains("Public contract"));
    }

    [Fact]
    public void DetectsRouteMappingChangesInPatch()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Program.cs", patch: "app.MapPost(\"/orders\", CreateOrder);"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.PublicContract
            && f.Points == 15
            && f.Title.Contains("route mapping"));
    }

    [Fact]
    public void DetectsOpenApiFileChanges()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("docs/OpenApi/v1.json"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.PublicContract
            && f.Points == 20);
    }
}
