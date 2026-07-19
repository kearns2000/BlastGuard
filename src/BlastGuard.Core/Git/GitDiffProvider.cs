using BlastGuard.Core.ChangeSets;

namespace BlastGuard.Core.Git;

public sealed class GitDiffProvider(IGitProcessRunner gitRunner) : IGitDiffProvider
{
    public async Task<PullRequestChangeSet> GetChangeSetAsync(
        string repositoryPath,
        string baseRef,
        string headRef,
        CancellationToken cancellationToken)
    {
        var fullRepoPath = Path.GetFullPath(repositoryPath);

        if (!Directory.Exists(fullRepoPath))
        {
            throw new BlastGuardGitException($"BlastGuard could not find a git repository at: {fullRepoPath}");
        }

        ValidateRef(baseRef);
        ValidateRef(headRef);

        var gitDirCheck = await gitRunner.RunAsync(fullRepoPath, ["rev-parse", "--git-dir"], cancellationToken);
        if (gitDirCheck.ExitCode != 0)
        {
            throw new BlastGuardGitException($"BlastGuard could not find a git repository at: {fullRepoPath}");
        }

        var range = $"{baseRef}...{headRef}";

        var nameStatus = await gitRunner.RunAsync(
            fullRepoPath,
            ["diff", "--name-status", "--find-renames", range],
            cancellationToken);

        if (nameStatus.ExitCode != 0)
        {
            throw new BlastGuardGitException(
                $"BlastGuard could not compare refs: {baseRef}...{headRef}",
                nameStatus.StandardError.Trim());
        }

        var numStat = await gitRunner.RunAsync(
            fullRepoPath,
            ["diff", "--numstat", "--find-renames", range],
            cancellationToken);

        if (numStat.ExitCode != 0)
        {
            throw new BlastGuardGitException(
                $"BlastGuard could not compare refs: {baseRef}...{headRef}",
                numStat.StandardError.Trim());
        }

        var patchOutput = await gitRunner.RunAsync(
            fullRepoPath,
            ["diff", "--unified=0", "--find-renames", range],
            cancellationToken);

        if (patchOutput.ExitCode != 0)
        {
            throw new BlastGuardGitException(
                $"BlastGuard could not compare refs: {baseRef}...{headRef}",
                patchOutput.StandardError.Trim());
        }

        var numStatByPath = ParseNumStat(numStat.StandardOutput);
        var patchesByPath = ParsePatches(patchOutput.StandardOutput);
        var files = ParseNameStatus(nameStatus.StandardOutput, numStatByPath, patchesByPath);

        return new PullRequestChangeSet(fullRepoPath, baseRef, headRef, files);
    }

    internal static void ValidateRef(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            throw new BlastGuardGitException("BlastGuard received an empty git ref.");
        }

        if (reference.StartsWith('-'))
        {
            throw new BlastGuardGitException($"BlastGuard received an invalid git ref: {reference}");
        }
    }

    internal static List<ChangedFile> ParseNameStatus(
        string nameStatusOutput,
        Dictionary<string, (int? Added, int? Deleted)> numStatByPath,
        Dictionary<string, string> patchesByPath)
    {
        var files = new List<ChangedFile>();

        foreach (var line in nameStatusOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = line.Split('\t');
            if (parts.Length < 2)
            {
                continue;
            }

            var statusCode = parts[0];
            var status = MapStatus(statusCode);
            string path;
            string? previousPath = null;

            if (statusCode.StartsWith('R') || statusCode.StartsWith('C'))
            {
                if (parts.Length < 3)
                {
                    continue;
                }

                previousPath = parts[1];
                path = parts[2];
            }
            else
            {
                path = parts[1];
            }

            numStatByPath.TryGetValue(path, out var lineCounts);
            patchesByPath.TryGetValue(path, out var patch);

            files.Add(new ChangedFile(
                path,
                status,
                lineCounts.Added,
                lineCounts.Deleted,
                previousPath,
                patch));
        }

        return files;
    }

    internal static Dictionary<string, (int? Added, int? Deleted)> ParseNumStat(string numStatOutput)
    {
        var result = new Dictionary<string, (int? Added, int? Deleted)>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in numStatOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = line.Split('\t');
            if (parts.Length < 3)
            {
                continue;
            }

            int? added = parts[0] == "-" ? null : int.TryParse(parts[0], out var a) ? a : (int?)null;
            int? deleted = parts[1] == "-" ? null : int.TryParse(parts[1], out var d) ? d : (int?)null;
            var path = ResolveNumStatPath(parts[2]);

            result[path] = (added, deleted);
        }

        return result;
    }

    /// <summary>
    /// Git numstat reports renames as "old => new" or "dir/{old => new}".
    /// Lookup keys must use the destination path so they match name-status.
    /// </summary>
    internal static string ResolveNumStatPath(string path)
    {
        const string arrow = " => ";
        var arrowIndex = path.IndexOf(arrow, StringComparison.Ordinal);
        if (arrowIndex < 0)
        {
            return path;
        }

        var braceStart = path.IndexOf('{');
        var braceEnd = path.IndexOf('}', braceStart + 1);
        if (braceStart >= 0 && braceEnd > braceStart && arrowIndex > braceStart && arrowIndex < braceEnd)
        {
            var prefix = path[..braceStart];
            var suffix = path[(braceEnd + 1)..];
            var newName = path[(arrowIndex + arrow.Length)..braceEnd];
            return prefix + newName + suffix;
        }

        return path[(arrowIndex + arrow.Length)..];
    }

    internal static Dictionary<string, string> ParsePatches(string patchOutput)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(patchOutput))
        {
            return result;
        }

        var lines = patchOutput.Split('\n');
        var currentPath = (string?)null;
        var currentPatch = new List<string>();

        void Flush()
        {
            if (currentPath is not null && currentPatch.Count > 0)
            {
                result[currentPath] = string.Join('\n', currentPatch);
            }

            currentPatch.Clear();
        }

        foreach (var line in lines)
        {
            if (line.StartsWith("diff --git ", StringComparison.Ordinal))
            {
                Flush();
                currentPath = ExtractPathFromDiffHeader(line);
                currentPatch.Add(line);
            }
            else if (currentPath is not null)
            {
                currentPatch.Add(line);
            }
        }

        Flush();
        return result;
    }

    private static string? ExtractPathFromDiffHeader(string line)
    {
        // diff --git a/path b/path
        var parts = line.Split(' ');
        if (parts.Length < 4)
        {
            return null;
        }

        var bPath = parts[3];
        return bPath.StartsWith("b/", StringComparison.Ordinal) ? bPath[2..] : bPath;
    }

    private static FileChangeStatus MapStatus(string statusCode)
    {
        var code = statusCode.Length > 0 ? statusCode[0] : '?';
        return code switch
        {
            'A' => FileChangeStatus.Added,
            'M' => FileChangeStatus.Modified,
            'D' => FileChangeStatus.Deleted,
            'R' => FileChangeStatus.Renamed,
            'C' => FileChangeStatus.Copied,
            'T' => FileChangeStatus.Modified,
            _ => FileChangeStatus.Unknown
        };
    }
}

public sealed class BlastGuardGitException(string message, string? details = null) : Exception(message)
{
    public string? Details { get; } = details;
}
