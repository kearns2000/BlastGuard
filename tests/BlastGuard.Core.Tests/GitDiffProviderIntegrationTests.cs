using BlastGuard.Core.Git;

namespace BlastGuard.Core.Tests;

public class GitDiffProviderIntegrationTests
{
    [Fact]
    public async Task GetChangeSetAsync_ReadsRenameLineCountsFromRealGit()
    {
        var repo = Path.Combine(Path.GetTempPath(), $"blastguard-git-{Guid.NewGuid():N}");
        Directory.CreateDirectory(repo);

        try
        {
            await RunGit(repo, "init");
            await RunGit(repo, "config", "user.email", "blastguard@example.com");
            await RunGit(repo, "config", "user.name", "BlastGuard");
            await File.WriteAllTextAsync(Path.Combine(repo, "old.txt"), "line1\n");
            await RunGit(repo, "add", "old.txt");
            await RunGit(repo, "commit", "-m", "initial");
            await RunGit(repo, "mv", "old.txt", "new.txt");
            await File.AppendAllTextAsync(Path.Combine(repo, "new.txt"), "line2\n");
            await RunGit(repo, "add", "new.txt");
            await RunGit(repo, "commit", "-m", "rename");

            var provider = new GitDiffProvider(new ProcessGitRunner());
            var changeSet = await provider.GetChangeSetAsync(repo, "HEAD~1", "HEAD", CancellationToken.None);

            var renamed = Assert.Single(changeSet.Files);
            Assert.Equal(ChangeSets.FileChangeStatus.Renamed, renamed.Status);
            Assert.Equal("new.txt", renamed.Path);
            Assert.Equal("old.txt", renamed.PreviousPath);
            Assert.True(renamed.LinesAdded is > 0);
        }
        finally
        {
            if (Directory.Exists(repo))
            {
                Directory.Delete(repo, recursive: true);
            }
        }
    }

    private static async Task RunGit(string repo, params string[] args)
    {
        var runner = new ProcessGitRunner();
        var result = await runner.RunAsync(repo, args, CancellationToken.None);
        Assert.True(result.ExitCode == 0, result.StandardError);
    }
}
