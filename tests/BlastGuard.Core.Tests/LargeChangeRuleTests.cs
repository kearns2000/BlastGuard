namespace BlastGuard.Core.Tests;

public class LargeChangeRuleTests
{
    [Fact]
    public void DetectsLargeLineCounts()
    {
        var report = TestFixtures.Score(
            TestFixtures.File("src/Large/File1.cs", linesAdded: 600, linesDeleted: 0));

        Assert.Contains(report.Findings, f => f.Points == 10 && f.Message.Contains("500"));
    }

    [Fact]
    public void DetectsLargeFileCounts()
    {
        var files = Enumerable.Range(1, 30)
            .Select(i => TestFixtures.File($"src/File{i}.cs", linesAdded: 1, linesDeleted: 0))
            .ToArray();

        var report = TestFixtures.Score(files);

        Assert.Contains(report.Findings, f => f.Points == 10 && f.Message.Contains("25 files"));
    }
}
