using BlastGuard.Core.Rules;

namespace BlastGuard.Core.Tests;

public class RuleHelpersTests
{
    [Fact]
    public void IsTestFile_DoesNotMatchTestingProjectPaths()
    {
        Assert.False(RuleHelpers.IsTestFile("src/MyCompany.Testing/Helpers/Thing.cs"));
    }

    [Fact]
    public void IsTestFile_MatchesStandardTestPaths()
    {
        Assert.True(RuleHelpers.IsTestFile("tests/Api/OrdersControllerTests.cs"));
        Assert.True(RuleHelpers.IsTestFile("src/Api.Tests/ThingTest.cs"));
    }

    [Fact]
    public void PathContainsSegment_DoesNotMatchSubstring()
    {
        Assert.False(RuleHelpers.PathContainsSegment("src/Billing/ContractorManagement/foo.cs", "Contract"));
        Assert.True(RuleHelpers.PathContainsSegment("src/Contracts/OrderResponse.cs", "Contracts"));
    }
}
