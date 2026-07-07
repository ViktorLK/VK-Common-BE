using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Scoring.Strategies;

/// <summary>
/// Scoring strategy based on emotional impact.
/// </summary>
internal sealed class EmotionalImpactScoringStrategy : IVKScoringStrategy
{
    public Task<VKResult<double>> ScoreAsync(string content, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(content);
        // Boilerplate placeholder logic
        return Task.FromResult(VKResult.Success(1.0));
    }
}
