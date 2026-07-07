using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Scoring.Strategies;

/// <summary>
/// Default scoring strategy that returns a zero/neutral score.
/// </summary>
internal sealed class NullScoringStrategy : IVKScoringStrategy
{
    public Task<VKResult<double>> ScoreAsync(string content, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(content);
        return Task.FromResult(VKResult.Success(0.0));
    }
}
