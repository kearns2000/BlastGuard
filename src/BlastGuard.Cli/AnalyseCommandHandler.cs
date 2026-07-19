using System.Text.Json;
using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Configuration;
using BlastGuard.Core.Formatting;
using BlastGuard.Core.Git;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Cli;

public sealed class AnalyseCommandHandler(
    IGitDiffProvider gitDiffProvider,
    BlastGuardScorer scorer)
{
    public async Task<int> ExecuteAsync(AnalyseOptions options, CancellationToken cancellationToken)
    {
        try
        {
            BlastGuardConfiguration configuration;
            try
            {
                var configPath = ConfigurationLoader.ResolveConfigPath(options.Config, options.Repo);
                if (configPath is not null && !File.Exists(configPath))
                {
                    Console.Error.WriteLine($"BlastGuard could not read configuration file: {options.Config ?? "blastguard.json"}");
                    return 1;
                }

                configuration = ConfigurationLoader.Load(options.Config, options.Repo);
            }
            catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException)
            {
                Console.Error.WriteLine($"BlastGuard could not read configuration file: {options.Config ?? "blastguard.json"}");
                if (options.Verbose)
                {
                    Console.Error.WriteLine(ex.Message);
                }

                return 1;
            }

            PullRequestChangeSet changeSet;
            try
            {
                changeSet = await gitDiffProvider.GetChangeSetAsync(
                    options.Repo,
                    options.Base,
                    options.Head,
                    cancellationToken);
            }
            catch (BlastGuardGitException ex)
            {
                Console.Error.WriteLine(ex.Message);
                if (options.Verbose && !string.IsNullOrWhiteSpace(ex.Details))
                {
                    Console.Error.WriteLine(ex.Details);
                }

                return 1;
            }

            var report = scorer.Score(changeSet, configuration);
            var formatter = ReportFormatterFactory.Create(options.Format);
            var output = formatter.Format(report, options.IncludeSuggestions);

            if (!string.IsNullOrWhiteSpace(options.Output))
            {
                var fullOutputPath = Path.GetFullPath(options.Output);
                var outputDirectory = Path.GetDirectoryName(fullOutputPath);
                if (!string.IsNullOrWhiteSpace(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                await File.WriteAllTextAsync(fullOutputPath, output + Environment.NewLine, cancellationToken);
            }
            else
            {
                Console.WriteLine(output);
            }

            if (options.FailThreshold.HasValue && report.Score >= options.FailThreshold.Value)
            {
                return 2;
            }

            return 0;
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
        catch (Exception ex) when (options.Verbose)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
        catch (Exception)
        {
            Console.Error.WriteLine("BlastGuard encountered an unexpected error.");
            return 1;
        }
    }
}

public sealed class AnalyseOptions
{
    public string Repo { get; init; } = ".";

    public string Base { get; init; } = "origin/main";

    public string Head { get; init; } = "HEAD";

    public string Format { get; init; } = "text";

    public string? Output { get; init; }

    public string? Config { get; init; }

    public int? FailThreshold { get; init; }

    public bool IncludeSuggestions { get; init; } = true;

    public bool Verbose { get; init; }
}
