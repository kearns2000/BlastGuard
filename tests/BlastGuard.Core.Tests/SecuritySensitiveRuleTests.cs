using BlastGuard.Core.Configuration;

namespace BlastGuard.Core.Tests;

public class SecuritySensitiveRuleTests
{
    [Fact]
    public void DetectsAuthenticationChanges()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Auth/JwtAuthenticationExtensions.cs", patch: "services.AddAuthentication();"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.Security
            && f.Points >= 20);
    }

    [Fact]
    public void DetectsAuthorisationChanges()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Security/PermissionsPolicy.cs", patch: "services.AddAuthorization();"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.Security
            && f.Title.Contains("Authorisation"));
    }

    [Fact]
    public void DoesNotFlagAuthorAsAuthentication()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Blog/AuthorController.cs", patch: "public string AuthorName { get; set; }"));

        Assert.DoesNotContain(report.Findings, f => f.Category == Scoring.RiskCategory.Security);
    }

    [Fact]
    public void DoesNotFlagDequeueAsQueueSecurity()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Blog/PostRepository.cs", patch: "var authored = posts.Where(p => p.Authored);"));

        Assert.DoesNotContain(report.Findings, f => f.Category == Scoring.RiskCategory.Security);
    }

    [Fact]
    public void CustomSecurityHintTriggersFinding()
    {
        var configuration = new BlastGuardConfiguration
        {
            SecurityPathHints = ["Tenant"]
        };

        var changeSet = TestFixtures.CreateChangeSet(
            TestFixtures.File("src/Tenant/TenantResolver.cs"));

        var report = Scoring.BlastGuardScorer.CreateDefault().Score(changeSet, configuration);

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.Security
            && f.Message.Contains("matched hint: Tenant"));
    }

    [Fact]
    public void DetectsCorsPolicy()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Api/Startup.cs", patch: "services.AddCors(options => {});"));

        Assert.Contains(report.Findings, f =>
            f.Category == Scoring.RiskCategory.Security
            && f.Title.Contains("CORS"));
    }
}
