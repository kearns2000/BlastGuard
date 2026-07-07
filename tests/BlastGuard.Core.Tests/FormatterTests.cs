using BlastGuard.Core.Formatting;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Tests;

public class FormatterTests
{
  private static BlastRadiusReport SampleReport() => new(
      76,
      RiskLevel.High,
      [
          new RiskFinding(RiskCategory.PublicContract, "public-contract", "Response DTO changed", "Public contract file changed.", 20),
          new RiskFinding(RiskCategory.Database, "database", "EF Core migration changed", "Migration changed.", 20),
          new RiskFinding(RiskCategory.Configuration, "configuration", "Production settings changed", "Production settings changed.", 15)
      ],
      [
          "Check whether API consumers are affected.",
          "Confirm the migration is backwards compatible."
      ],
      new ReportSummary(5, 120, 30, ["Response DTO changed", "EF Core migration changed"]));

  [Fact]
  public void JsonFormatter_ProducesIndentedOutput()
  {
    var output = new JsonReportFormatter().Format(SampleReport());

    Assert.Contains("\"score\": 76", output);
    Assert.Contains("\"risk\": \"High\"", output);
    Assert.Contains("suggestedReviewFocus", output);
    Assert.Contains("  ", output);
  }

  [Fact]
  public void MarkdownFormatter_IncludesTableAndSuggestions()
  {
    var output = new MarkdownReportFormatter().Format(SampleReport());

    Assert.Contains("# BlastGuard report", output);
    Assert.Contains("| Public contract | Response DTO changed | +20 |", output);
    Assert.Contains("## Suggested review focus", output);
    Assert.Contains("- Confirm the migration is backwards compatible.", output);
  }

  [Fact]
  public void GitHubFormatter_IncludesCompactTable()
  {
    var output = new GitHubReportFormatter().Format(SampleReport());

    Assert.Contains("## BlastGuard report", output);
    Assert.Contains("**Score:** 76 / 100", output);
    Assert.Contains("| Database | EF Core migration changed | +20 |", output);
    Assert.Contains("### Suggested review focus", output);
  }

  [Fact]
  public void TextFormatter_IncludesMainSignals()
  {
    var output = new TextReportFormatter().Format(SampleReport());

    Assert.Contains("BlastGuard report", output);
    Assert.Contains("Score: 76 / 100", output);
    Assert.Contains("Risk: High", output);
    Assert.Contains("Main risk signals:", output);
    Assert.Contains("Suggested review focus:", output);
  }
}
