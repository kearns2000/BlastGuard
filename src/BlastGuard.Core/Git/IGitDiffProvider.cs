using BlastGuard.Core.ChangeSets;

namespace BlastGuard.Core.Git;

public interface IGitDiffProvider
{
    Task<PullRequestChangeSet> GetChangeSetAsync(
        string repositoryPath,
        string baseRef,
        string headRef,
        CancellationToken cancellationToken);
}

public interface IGitProcessRunner
{
    Task<GitProcessResult> RunAsync(
        string workingDirectory,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken);
}

public sealed record GitProcessResult(int ExitCode, string StandardOutput, string StandardError);
