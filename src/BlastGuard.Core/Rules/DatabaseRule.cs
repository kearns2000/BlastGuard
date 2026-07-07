using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Configuration;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Rules;

public sealed class DatabaseRule : IBlastGuardRule
{
  private static readonly string[] MigrationIndicators = ["Migrations", "migrationBuilder"];

  private static readonly string[] DbContextIndicators = ["DbContext"];

  private static readonly string[] EntityConfigIndicators =
  [
      "IEntityTypeConfiguration", "EntityTypeBuilder", "HasColumnType", "HasMaxLength", "IsRequired"
  ];

  private static readonly string[] DestructiveIndicators =
  [
      "DropTable", "DropColumn", "RenameColumn", "RenameTable", "AlterColumn",
      "DROP TABLE", "DROP COLUMN", "ALTER TABLE"
  ];

  public string Id => "database";

  public IEnumerable<RiskFinding> Analyse(
      PullRequestChangeSet changeSet,
      BlastGuardConfiguration configuration)
  {
    if (!RuleHelpers.IsRuleEnabled(configuration, Id))
    {
      yield break;
    }

    foreach (var file in RuleHelpers.GetRelevantFiles(changeSet, configuration))
    {
      var path = PathMatcher.NormalisePath(file.Path);
      var isMigration = RuleHelpers.PathContainsAny(path, MigrationIndicators)
          || path.Contains("/Migrations/", StringComparison.OrdinalIgnoreCase);

      if (isMigration)
      {
        yield return new RiskFinding(
            RiskCategory.Database,
            Id,
            "EF Core migration changed",
            $"EF Core migration added or modified: {path}.",
            20,
            file.Path);

        if (RuleHelpers.PatchContainsAny(file.Patch, DestructiveIndicators))
        {
          yield return new RiskFinding(
              RiskCategory.Database,
              Id,
              "Potential destructive migration",
              $"Potential destructive migration detected in: {path}.",
              30,
              file.Path);
        }

        continue;
      }

      if (path.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
      {
        yield return new RiskFinding(
            RiskCategory.Database,
            Id,
            "Raw SQL changed",
            $"Raw SQL changed: {path}.",
            20,
            file.Path);

        if (RuleHelpers.PatchContainsAny(file.Patch, DestructiveIndicators))
        {
          yield return new RiskFinding(
              RiskCategory.Database,
              Id,
              "Potential destructive SQL change",
              $"Potential destructive SQL change detected in: {path}.",
              30,
              file.Path);
        }

        continue;
      }

      if (RuleHelpers.PathContainsAny(path, DbContextIndicators))
      {
        yield return new RiskFinding(
            RiskCategory.Database,
            Id,
            "DbContext changed",
            $"DbContext changed: {path}.",
            15,
            file.Path);
        continue;
      }

      if (RuleHelpers.PathContainsAny(path, EntityConfigIndicators)
          || RuleHelpers.PatchContainsAny(file.Patch, EntityConfigIndicators))
      {
        yield return new RiskFinding(
            RiskCategory.Database,
            Id,
            "Entity configuration changed",
            $"Entity configuration changed: {path}.",
            10,
            file.Path);
      }
    }
  }
}
