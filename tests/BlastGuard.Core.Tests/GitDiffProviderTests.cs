using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Git;

namespace BlastGuard.Core.Tests;

public class GitDiffProviderTests
{
    [Fact]
    public void ParseNameStatus_HandlesRenamesAndLineCounts()
    {
        var nameStatus = "R100\tsrc/Old.cs\tsrc/New.cs\nM\tsrc/Modified.cs";
        var numStat = new Dictionary<string, (int? Added, int? Deleted)>
        {
            ["src/New.cs"] = (10, 5),
            ["src/Modified.cs"] = (3, 1)
        };
        var patches = new Dictionary<string, string>
        {
            ["src/New.cs"] = "diff --git a/src/Old.cs b/src/New.cs",
            ["src/Modified.cs"] = "diff --git a/src/Modified.cs b/src/Modified.cs"
        };

        var files = GitDiffProvider.ParseNameStatus(nameStatus, numStat, patches);

        Assert.Equal(2, files.Count);
        Assert.Equal(FileChangeStatus.Renamed, files[0].Status);
        Assert.Equal("src/Old.cs", files[0].PreviousPath);
        Assert.Equal(10, files[0].LinesAdded);
        Assert.Equal(FileChangeStatus.Modified, files[1].Status);
    }

    [Fact]
    public void ParseNumStat_TreatsBinaryDashAsNull()
    {
        var result = GitDiffProvider.ParseNumStat("-\t-\tsrc/image.png\n10\t2\tsrc/File.cs");

        Assert.Null(result["src/image.png"].Added);
        Assert.Null(result["src/image.png"].Deleted);
        Assert.Equal(10, result["src/File.cs"].Added);
        Assert.Equal(2, result["src/File.cs"].Deleted);
    }

    [Theory]
    [InlineData("old.txt => new.txt", "new.txt")]
    [InlineData("src/{Old.cs => New.cs}", "src/New.cs")]
    [InlineData("src/Unchanged.cs", "src/Unchanged.cs")]
    public void ResolveNumStatPath_UsesDestinationPathForRenames(string input, string expected)
    {
        Assert.Equal(expected, GitDiffProvider.ResolveNumStatPath(input));
    }

    [Fact]
    public void ParseNumStat_MapsRenamePathsToDestination()
    {
        var result = GitDiffProvider.ParseNumStat("1\t0\told.txt => new.txt\n3\t1\tsrc/{Old.cs => New.cs}");

        Assert.Equal(1, result["new.txt"].Added);
        Assert.Equal(0, result["new.txt"].Deleted);
        Assert.Equal(3, result["src/New.cs"].Added);
        Assert.Equal(1, result["src/New.cs"].Deleted);
        Assert.False(result.ContainsKey("old.txt => new.txt"));
    }

    [Fact]
    public void ParseNameStatus_JoinsRenameLineCountsFromNumStat()
    {
        var nameStatus = "R050\told.txt\tnew.txt";
        var numStat = GitDiffProvider.ParseNumStat("1\t0\told.txt => new.txt");
        var patches = new Dictionary<string, string>
        {
            ["new.txt"] = "diff --git a/old.txt b/new.txt"
        };

        var files = GitDiffProvider.ParseNameStatus(nameStatus, numStat, patches);

        Assert.Single(files);
        Assert.Equal(FileChangeStatus.Renamed, files[0].Status);
        Assert.Equal(1, files[0].LinesAdded);
        Assert.Equal(0, files[0].LinesDeleted);
    }

    [Theory]
    [InlineData("--upload-pack=/tmp/evil.sh")]
    [InlineData("-x")]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateRef_RejectsUnsafeRefs(string reference)
    {
        Assert.Throws<BlastGuardGitException>(() => GitDiffProvider.ValidateRef(reference));
    }

    [Theory]
    [InlineData("main")]
    [InlineData("origin/main")]
    [InlineData("HEAD")]
    [InlineData("feature/add-thing")]
    public void ValidateRef_AllowsNormalRefs(string reference)
    {
        var exception = Record.Exception(() => GitDiffProvider.ValidateRef(reference));
        Assert.Null(exception);
    }

    [Fact]
    public void ParsePatches_GroupsByFilePath()
    {
        var patch = """
            diff --git a/src/A.cs b/src/A.cs
            @@ -1 +1 @@
            -old
            +new
            diff --git a/src/B.cs b/src/B.cs
            @@ -1 +1 @@
            -foo
            +bar
            """;

        var result = GitDiffProvider.ParsePatches(patch);

        Assert.Equal(2, result.Count);
        Assert.Contains("src/A.cs", result.Keys);
        Assert.Contains("old", result["src/A.cs"]);
        Assert.Contains("src/B.cs", result.Keys);
    }
}
