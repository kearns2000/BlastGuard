using BlastGuard.Core.ChangeSets;
using BlastGuard.Core.Configuration;
using BlastGuard.Core.Scoring;

namespace BlastGuard.Core.Rules;

public interface IBlastGuardRule
{
    string Id { get; }

    IEnumerable<RiskFinding> Analyse(
        PullRequestChangeSet changeSet,
        BlastGuardConfiguration configuration);
}
