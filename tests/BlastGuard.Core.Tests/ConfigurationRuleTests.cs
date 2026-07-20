namespace BlastGuard.Core.Tests;

public class ConfigurationRuleTests
{
    [Fact]
    public void DetectsProductionAppsettingsChanges()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Api/appsettings.Production.json"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.Configuration
            && f.Points == 15
            && f.Title.Contains("Production"));
    }

    [Fact]
    public void DetectsInfrastructureConfigurationChanges()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("infra/main.bicep"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.Infrastructure
            && f.Points == 15);
    }

    [Fact]
    public void DetectsInfraYamlUnderInfraRoot()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("infra/api-deployment.yaml"));

        Assert.Contains(report.Findings, f => f.Category == Scoring.RiskCategory.Infrastructure);
    }

    [Fact]
    public void DoesNotFlagGitHubWorkflowYamlAsInfrastructure()
    {
        var report = TestFixtures.Score(
            TestFixtures.File(".github/workflows/ci.yml"));

        Assert.DoesNotContain(report.Findings, f => f.Category == Scoring.RiskCategory.Infrastructure);
    }

    [Fact]
    public void DoesNotFlagArbitraryYamlAsInfrastructure()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("docs/examples/sample.yaml"));

        Assert.DoesNotContain(report.Findings, f => f.Category == Scoring.RiskCategory.Infrastructure);
    }

    [Fact]
    public void DoesNotTreatProductAppsettingsAsProduction()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Api/appsettings.ProductCatalog.json"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.Configuration
            && f.Title.Contains("Appsettings")
            && !f.Title.Contains("Production"));
        Assert.DoesNotContain(report.Findings, f => f.Title.Contains("Production appsettings"));
    }

    [Fact]
    public void DetectsOptionsClassByFileNameSuffix()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Api/PaymentOptions.cs"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.Configuration
            && f.Title.Contains("Options class"));
    }

    [Fact]
    public void DetectsOptionsClassByPathSegment()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Configuration/ServiceRegistration.cs"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.Configuration
            && f.Title.Contains("Options class"));
    }

    [Fact]
    public void DoesNotFlagSettingsTestFilesAsOptionsClass()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("tests/Api/SettingsTests.cs"));

        Assert.DoesNotContain(report.Findings, f => f.Title.Contains("Options class"));
    }

    [Fact]
    public void DoesNotFlagAppsettingsJsonAsOptionsClass()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Api/appsettings.json"));

        Assert.Single(report.Findings, f => f.Category == Scoring.RiskCategory.Configuration);
        Assert.DoesNotContain(report.Findings, f => f.Title.Contains("Options class"));
    }
}
