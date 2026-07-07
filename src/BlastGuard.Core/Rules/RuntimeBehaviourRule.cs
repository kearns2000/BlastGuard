using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Configuration;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Rules;

public sealed class RuntimeBehaviourRule : IBlastGuardRule
{
  private static readonly (string[] Indicators, string Title, string Message, int Points)[] BehaviourGroups =
  [
      (["BackgroundService", "IHostedService", "HostedService", "PeriodicTimer"],
          "Background worker changed", "Background worker changed in: {0}.", 15),
      (["Channel<", "Queue", "ServiceBus", "RabbitMQ", "Kafka", "Hangfire", "Quartz"],
          "Queue or message handling changed", "Queue or message handling changed in: {0}.", 15),
      (["Retry", "Retries", "Polly", "CircuitBreaker", "Timeout"],
          "Retry or timeout behaviour changed", "Retry or timeout behaviour changed in: {0}.", 15),
      (["Cache", "Caching", "IMemoryCache", "IDistributedCache"],
          "Caching changed", "Caching changed in: {0}.", 10),
      (["Task.Run", "Parallel.ForEach", "SemaphoreSlim", "lock (", "Monitor.Enter"],
          "Concurrency-sensitive code changed", "Concurrency-sensitive code changed in: {0}.", 15),
      (["CancellationToken"],
          "CancellationToken handling changed", "CancellationToken handling changed in: {0}.", 10),
      (["HttpClient", "AddHttpClient"],
          "HttpClient configuration changed", "HttpClient configuration changed in: {0}.", 10)
  ];

  public string Id => "runtime-behaviour";

  public IEnumerable<RiskFinding> Analyse(
      PullRequestChangeSet changeSet,
      BlastGuardConfiguration configuration)
  {
    if (!RuleHelpers.IsRuleEnabled(configuration, Id))
    {
      yield break;
    }

    var reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (var file in RuleHelpers.GetRelevantFiles(changeSet, configuration))
    {
      var path = PathMatcher.NormalisePath(file.Path);
      var searchText = path + (file.Patch ?? string.Empty);

      foreach (var (indicators, title, messageTemplate, points) in BehaviourGroups)
      {
        var key = $"{file.Path}:{title}";
        if (reported.Contains(key))
        {
          continue;
        }

        if (indicators.Any(indicator => searchText.Contains(indicator, StringComparison.Ordinal)))
        {
          reported.Add(key);
          yield return new RiskFinding(
              RiskCategory.RuntimeBehaviour,
              Id,
              title,
              string.Format(messageTemplate, path),
              points,
              file.Path);
        }
      }
    }
  }
}
