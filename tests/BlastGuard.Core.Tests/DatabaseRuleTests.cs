namespace BlastGuard.Core.Tests;

public class DatabaseRuleTests
{
    [Fact]
    public void DetectsEfCoreMigrationChanges()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Infrastructure/Migrations/20240101000000_Initial.cs"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.Database
            && f.Points == 20
            && f.Title.Contains("migration"));
    }

    [Fact]
    public void DetectsDestructiveMigrationChanges()
    {
        var report = TestFixtures.Score(
            TestFixtures.File(
                "src/Infrastructure/Migrations/20240102000000_DropUsers.cs",
                patch: "migrationBuilder.DropTable(name: \"Users\");"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.Database
            && f.Points == 30
            && f.Title.Contains("destructive"));
    }

    [Fact]
    public void DetectsDbContextChanges()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Data/AppDbContext.cs"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.Database
            && f.Points == 15);
    }
}
