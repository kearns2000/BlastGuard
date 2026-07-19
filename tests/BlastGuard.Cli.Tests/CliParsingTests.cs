using System.CommandLine;

namespace BlastGuard.Cli.Tests;

public class CliParsingTests
{
    [Theory]
    [InlineData("analyse")]
    [InlineData("analyze")]
    public async Task AnalyseAlias_IsRegistered(string commandName)
    {
        var root = Program.CreateRootCommand();
        var exitCode = await root.InvokeAsync([commandName, "--help"]);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task UnknownCommand_ReturnsNonZero()
    {
        var root = Program.CreateRootCommand();
        var exitCode = await root.InvokeAsync(["unknown-command"]);

        Assert.NotEqual(0, exitCode);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    public void IncludeSuggestions_AcceptsBooleanLiterals(string value)
    {
        var root = Program.CreateRootCommand();
        var parseResult = root.Parse(["analyse", "--include-suggestions", value]);

        Assert.Empty(parseResult.Errors);
    }
}
