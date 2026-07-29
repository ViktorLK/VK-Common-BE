using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Scoring.Internal;

/// <summary>
/// Task for default scoring fallback (returns 0.0).
/// Order = 100.
/// </summary>
internal sealed class NullScoringTask : IVKScoringTask
{
    public VKPipelineSchedule Schedule => new(100);

    public Task<VKResult<VKScoringResult>> ExecuteAsync(VKScoringContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);
        return Task.FromResult(VKResult.Success(VKScoringResult.SuccessScore(0.0)));
    }
}
