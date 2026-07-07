namespace BlastGuard.Core.ChangeSets;

public sealed record PullRequestChangeSet(
    string RepositoryPath,
    string BaseRef,
    string HeadRef,
    IReadOnlyList<ChangedFile> Files);

public sealed record ChangedFile(
    string Path,
    FileChangeStatus Status,
    int? LinesAdded,
    int? LinesDeleted,
    string? PreviousPath,
    string? Patch);

public enum FileChangeStatus
{
    Added,
    Modified,
    Deleted,
    Renamed,
    Copied,
    Unknown
}
