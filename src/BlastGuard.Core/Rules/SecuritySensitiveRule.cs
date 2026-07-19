using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Configuration;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Rules;

public sealed class SecuritySensitiveRule : IBlastGuardRule
{
  private static readonly (string[] Indicators, string Title, string Message, int Points)[] SecurityGroups =
  [
      (["Auth", "Authentication", "AddAuthentication", "JwtBearer", "TokenValidationParameters"],
          "Authentication code touched", "Authentication code touched in: {0}.", 25),
      (["Authorization", "Authorisation", "AddAuthorization", "AddAuthorisation",
          "AuthorizationHandler", "AuthorisationHandler", "AuthorizationPolicy",
          "AuthorisationPolicy", "IAuthorizationRequirement"],
          "Authorisation code touched", "Authorisation code touched in: {0}.", 25),
      (["Claims", "ClaimsPrincipal"],
          "Claims mapping touched", "Claims mapping touched in: {0}.", 20),
      (["CORS", "Cors"],
          "CORS policy touched", "CORS policy touched in: {0}.", 20),
      (["Secret", "Secrets", "KeyVault"],
          "Secrets or Key Vault code touched", "Secrets or Key Vault code touched in: {0}.", 20),
      (["Encryption", "Cryptography", "Password"],
          "Encryption or cryptography code touched", "Encryption or cryptography code touched in: {0}.", 25),
      (["Cookie", "Cookies"],
          "Cookie handling touched", "Cookie handling touched in: {0}.", 15)
  ];

  public string Id => "security";

  public IEnumerable<RiskFinding> Analyse(
      PullRequestChangeSet changeSet,
      BlastGuardConfiguration configuration)
  {
    if (!RuleHelpers.IsRuleEnabled(configuration, Id))
    {
      yield break;
    }

    var builtInIndicators = SecurityGroups
        .SelectMany(group => group.Indicators)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    var reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (var file in RuleHelpers.GetRelevantFiles(changeSet, configuration))
    {
      var path = PathMatcher.NormalisePath(file.Path);
      var searchText = path + "\n" + (file.Patch ?? string.Empty);

      foreach (var (groupIndicators, title, messageTemplate, points) in SecurityGroups)
      {
        var key = $"{file.Path}:{title}";
        if (reported.Contains(key))
        {
          continue;
        }

        if (IdentifierMatcher.ContainsAnyIdentifier(searchText, groupIndicators))
        {
          reported.Add(key);
          yield return new RiskFinding(
              RiskCategory.Security,
              Id,
              title,
              string.Format(messageTemplate, path),
              points,
              file.Path);
        }
      }

      foreach (var hint in configuration.SecurityPathHints)
      {
        if (builtInIndicators.Contains(hint))
        {
          continue;
        }

        if (!IdentifierMatcher.ContainsIdentifier(path, hint))
        {
          continue;
        }

        var key = $"{file.Path}:hint:{hint}";
        if (reported.Contains(key))
        {
          continue;
        }

        reported.Add(key);
        yield return new RiskFinding(
            RiskCategory.Security,
            Id,
            "Security-sensitive path touched",
            $"Security-sensitive path touched: {path} (matched hint: {hint}).",
            20,
            file.Path);
      }
    }
  }
}
