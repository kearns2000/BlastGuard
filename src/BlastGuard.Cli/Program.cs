using System.CommandLine;
using BlastGuard.Core.Git;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args) =>
        await CreateRootCommand().InvokeAsync(args);

    internal static RootCommand CreateRootCommand()
    {
        var repoOption = new Option<string>("--repo", () => ".", "Path to the git repository.");
        var baseOption = new Option<string>("--base", () => "main", "Base git ref to compare against.");
        var headOption = new Option<string>("--head", () => "HEAD", "Head git ref to analyse.");
        var formatOption = new Option<string>("--format", () => "text", "Output format: text, json, markdown, or github.");
        var outputOption = new Option<string?>("--output", "Optional output file path.");
        var configOption = new Option<string?>("--config", "Optional path to blastguard.json.");
        var failThresholdOption = new Option<int?>("--fail-threshold", "Exit with a non-zero code when the score meets or exceeds this value.");
        var includeSuggestionsOption = new Option<bool>("--include-suggestions", () => true, "Include suggested review focus in the output.");
        var verboseOption = new Option<bool>("--verbose", "Show technical error details.");

        var analyseCommand = new Command("analyse", "Analyse a git diff and score pull request blast radius.");
        var analyzeCommand = new Command("analyze", "Analyse a git diff and score pull request blast radius.");

        foreach (var command in new[] { analyseCommand, analyzeCommand })
        {
            command.AddOption(repoOption);
            command.AddOption(baseOption);
            command.AddOption(headOption);
            command.AddOption(formatOption);
            command.AddOption(outputOption);
            command.AddOption(configOption);
            command.AddOption(failThresholdOption);
            command.AddOption(includeSuggestionsOption);
            command.AddOption(verboseOption);

            command.SetHandler(async context =>
            {
                var options = new AnalyseOptions
                {
                    Repo = context.ParseResult.GetValueForOption(repoOption)!,
                    Base = context.ParseResult.GetValueForOption(baseOption)!,
                    Head = context.ParseResult.GetValueForOption(headOption)!,
                    Format = context.ParseResult.GetValueForOption(formatOption)!,
                    Output = context.ParseResult.GetValueForOption(outputOption),
                    Config = context.ParseResult.GetValueForOption(configOption),
                    FailThreshold = context.ParseResult.GetValueForOption(failThresholdOption),
                    IncludeSuggestions = context.ParseResult.GetValueForOption(includeSuggestionsOption),
                    Verbose = context.ParseResult.GetValueForOption(verboseOption)
                };

                var handler = new AnalyseCommandHandler(
                    new GitDiffProvider(new ProcessGitRunner()),
                    BlastGuardScorer.CreateDefault());

                context.ExitCode = await handler.ExecuteAsync(options, context.GetCancellationToken());
            });
        }

        var rootCommand = new RootCommand("BlastGuard is a lightweight pull request blast radius scorer for .NET repositories.");
        rootCommand.AddCommand(analyseCommand);
        rootCommand.AddCommand(analyzeCommand);

        return rootCommand;
    }
}
