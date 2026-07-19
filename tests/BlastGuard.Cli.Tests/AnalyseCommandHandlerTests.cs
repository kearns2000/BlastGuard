using BlastGuard.Cli;
using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Configuration;
using BlastGuard.Core.Git;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Cli.Tests;

public class AnalyseCommandHandlerTests
{
    [Fact]
    public async Task ReturnsNonZeroForInvalidFormat()
    {
        var handler = CreateHandler(new FakeGitDiffProvider());
        var exitCode = await handler.ExecuteAsync(new AnalyseOptions { Format = "xml" }, CancellationToken.None);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ReturnsNonZeroWhenGitRepositoryMissing()
    {
        var handler = CreateHandler(new ThrowingGitDiffProvider());
        var exitCode = await handler.ExecuteAsync(new AnalyseOptions(), CancellationToken.None);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ReturnsTwoWhenFailThresholdExceeded()
    {
        var handler = CreateHandler(new FakeGitDiffProvider(
            TestFixtures.CreateChangeSet(
                new ChangedFile("src/Controllers/OrdersController.cs", FileChangeStatus.Modified, 10, 5, null, "MapGet(\"/orders\");"))));

        var exitCode = await handler.ExecuteAsync(
            new AnalyseOptions { FailThreshold = 1, Format = "json" },
            CancellationToken.None);

        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task WritesOutputToFileWhenRequested()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"blastguard-{Guid.NewGuid():N}.md");
        try
        {
            var handler = CreateHandler(new FakeGitDiffProvider());
            var exitCode = await handler.ExecuteAsync(
                new AnalyseOptions { Format = "markdown", Output = outputPath },
                CancellationToken.None);

            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(outputPath));
            var content = await File.ReadAllTextAsync(outputPath);
            Assert.Contains("BlastGuard report", content);
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    [Fact]
    public async Task OmitsSuggestionsWhenIncludeSuggestionsIsFalse()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"blastguard-{Guid.NewGuid():N}.txt");
        try
        {
            var handler = CreateHandler(new FakeGitDiffProvider(
                TestFixtures.CreateChangeSet(
                    new ChangedFile(
                        "src/Controllers/OrdersController.cs",
                        FileChangeStatus.Modified,
                        10,
                        5,
                        null,
                        "MapGet(\"/orders\");"))));

            var exitCode = await handler.ExecuteAsync(
                new AnalyseOptions
                {
                    Format = "text",
                    Output = outputPath,
                    IncludeSuggestions = false
                },
                CancellationToken.None);

            Assert.Equal(0, exitCode);
            var content = await File.ReadAllTextAsync(outputPath);
            Assert.DoesNotContain("Suggested review focus", content);
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    [Fact]
    public async Task CreatesMissingOutputDirectories()
    {
        var outputDir = Path.Combine(Path.GetTempPath(), $"blastguard-out-{Guid.NewGuid():N}", "nested");
        var outputPath = Path.Combine(outputDir, "report.md");

        try
        {
            var handler = CreateHandler(new FakeGitDiffProvider());
            var exitCode = await handler.ExecuteAsync(
                new AnalyseOptions { Format = "markdown", Output = outputPath },
                CancellationToken.None);

            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(outputPath));
        }
        finally
        {
            var root = Directory.GetParent(outputDir)?.FullName;
            if (root is not null && Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static AnalyseCommandHandler CreateHandler(IGitDiffProvider gitDiffProvider) =>
        new(gitDiffProvider, BlastGuardScorer.CreateDefault());

    private sealed class FakeGitDiffProvider(PullRequestChangeSet? changeSet = null) : IGitDiffProvider
    {
        public Task<PullRequestChangeSet> GetChangeSetAsync(
            string repositoryPath,
            string baseRef,
            string headRef,
            CancellationToken cancellationToken) =>
            Task.FromResult(changeSet ?? new PullRequestChangeSet(repositoryPath, baseRef, headRef, []));
    }

    private sealed class ThrowingGitDiffProvider : IGitDiffProvider
    {
        public Task<PullRequestChangeSet> GetChangeSetAsync(
            string repositoryPath,
            string baseRef,
            string headRef,
            CancellationToken cancellationToken) =>
            throw new BlastGuardGitException($"BlastGuard could not find a git repository at: {repositoryPath}");
    }
}

internal static class TestFixtures
{
    public static PullRequestChangeSet CreateChangeSet(params ChangedFile[] files) =>
        new("/repo", "main", "HEAD", files);
}
