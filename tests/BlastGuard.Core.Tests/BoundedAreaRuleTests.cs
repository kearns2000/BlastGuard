using BlastGuard.Core.Configuration;
using BlastGuard.Core.Rules;

namespace BlastGuard.Core.Tests;

public class BoundedAreaRuleTests
{
    [Fact]
    public void SingleAreaAddsNoPoints()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Features/Payments/PaymentService.cs"),
            TestFixtures.File("src/Features/Payments/PaymentValidator.cs"));

        Assert.DoesNotContain(report.Findings, f => f.RuleId == "bounded-area");
    }

    [Theory]
    [InlineData(2, 5)]
    [InlineData(3, 10)]
    [InlineData(4, 15)]
    [InlineData(5, 25)]
    [InlineData(6, 25)]
    public void MultipleAreasAddExpectedPoints(int areaCount, int expectedPoints)
    {
        var files = Enumerable.Range(1, areaCount)
            .Select(i => TestFixtures.File($"src/Features/Area{i}/Service.cs"))
            .ToArray();

        var report = TestFixtures.Score(files);

        Assert.Contains(report.Findings, f =>
            f.RuleId == "bounded-area"
            && f.Category == Scoring.RiskCategory.Scope
            && f.Points == expectedPoints);
    }

    [Fact]
    public void MessageListsTouchedAreas()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Features/Payments/PaymentService.cs"),
            TestFixtures.File("src/Features/Customers/CustomerService.cs"));

        var finding = Assert.Single(report.Findings, f => f.RuleId == "bounded-area");
        Assert.Contains("2 bounded areas", finding.Message);
        Assert.Contains("Payments", finding.Message);
        Assert.Contains("Customers", finding.Message);
    }

    [Fact]
    public void InferArea_UsesMostSpecificRoot()
    {
        var roots = BlastGuardConfiguration.DefaultBoundedAreaRoots;

        Assert.Equal("src/Features/Payments", BoundedAreaRule.InferArea("src/Features/Payments/PaymentService.cs", roots));
        Assert.Equal("src/Modules/Billing", BoundedAreaRule.InferArea("src/Modules/Billing/Invoice.cs", roots));
        Assert.Equal("src/Api", BoundedAreaRule.InferArea("src/Api/Program.cs", roots));
    }

    [Fact]
    public void InferArea_ReturnsNullForUnmatchedPath()
    {
        var roots = BlastGuardConfiguration.DefaultBoundedAreaRoots;

        Assert.Null(BoundedAreaRule.InferArea("tests/UnitTests/ExampleTests.cs", roots));
    }
}
