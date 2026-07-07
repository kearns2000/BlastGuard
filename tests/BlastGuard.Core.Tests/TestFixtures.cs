using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Configuration;
using BlastGuard.Core.Rules;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Tests;

public static class TestFixtures
{
    public static PullRequestChangeSet CreateChangeSet(params ChangedFile[] files) =>
        new("/repo", "main", "HEAD", files);

    public static ChangedFile File(
        string path,
        string? patch = null,
        FileChangeStatus status = FileChangeStatus.Modified,
        int? linesAdded = 10,
        int? linesDeleted = 5) =>
        new(path, status, linesAdded, linesDeleted, null, patch);

    public static BlastGuardConfiguration DefaultConfiguration => new();

    public static BlastRadiusReport Score(params ChangedFile[] files) =>
        BlastGuardScorer.CreateDefault().Score(CreateChangeSet(files), DefaultConfiguration);
}
