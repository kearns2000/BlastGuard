using BlastGuard.Core.Rules;

namespace BlastGuard.Core.Tests;

public class IdentifierMatcherTests
{
    [Theory]
    [InlineData("services.AddAuthentication()", "AddAuthentication", true)]
    [InlineData("services.AddAuthentication()", "Authentication", true)]
    [InlineData("public string AuthorName", "Auth", false)]
    [InlineData("public string AuthorName", "Author", true)]
    [InlineData("src/Auth/Thing.cs", "Auth", true)]
    [InlineData("app.UseCors()", "Cors", true)]
    [InlineData("var deduped = Dequeue()", "Queue", false)]
    [InlineData("new TokenValidationParameters()", "TokenValidationParameters", true)]
    [InlineData("", "Auth", false)]
    public void ContainsIdentifier_RespectsWordBoundaries(string text, string indicator, bool expected)
    {
        Assert.Equal(expected, IdentifierMatcher.ContainsIdentifier(text, indicator));
    }

    [Fact]
    public void Tokenise_SplitsPascalCaseAndAcronyms()
    {
        Assert.Equal(["add", "authentication"], IdentifierMatcher.Tokenise("AddAuthentication"));
        Assert.Equal(["cors"], IdentifierMatcher.Tokenise("CORS"));
        Assert.Equal(["token", "validation", "parameters"], IdentifierMatcher.Tokenise("TokenValidationParameters"));
    }
}
