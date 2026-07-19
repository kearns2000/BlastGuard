using BlastGuard.Core.Configuration;

namespace BlastGuard.Core.Tests;

public class ConfigurationLoaderTests
{
    [Fact]
    public void ResolveConfigPath_ResolvesRelativePathAgainstRepository()
    {
        var repo = Path.Combine(Path.GetTempPath(), $"blastguard-repo-{Guid.NewGuid():N}");
        var previous = Directory.GetCurrentDirectory();

        try
        {
            Directory.CreateDirectory(repo);
            File.WriteAllText(Path.Combine(repo, "custom.json"), """{ "thresholds": { "medium": 30 } }""");

            var elsewhere = Path.Combine(Path.GetTempPath(), $"blastguard-cwd-{Guid.NewGuid():N}");
            Directory.CreateDirectory(elsewhere);
            Directory.SetCurrentDirectory(elsewhere);

            var resolved = ConfigurationLoader.ResolveConfigPath("custom.json", repo);

            Assert.Equal(Path.GetFullPath(Path.Combine(repo, "custom.json")), resolved);
        }
        finally
        {
            Directory.SetCurrentDirectory(previous);
            if (Directory.Exists(repo))
            {
                Directory.Delete(repo, recursive: true);
            }
        }
    }

    [Fact]
    public void ResolveConfigPath_KeepsAbsolutePaths()
    {
        var absolute = Path.GetFullPath(Path.Combine(Path.GetTempPath(), $"blastguard-{Guid.NewGuid():N}.json"));
        var resolved = ConfigurationLoader.ResolveConfigPath(absolute, Path.GetTempPath());

        Assert.Equal(absolute, resolved);
    }
}
