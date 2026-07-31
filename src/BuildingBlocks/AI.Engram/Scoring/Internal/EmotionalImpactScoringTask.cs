using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Scoring.Internal;

/// <summary>
/// Task for emotional impact engram scoring.
/// Order = 20.
/// </summary>
internal sealed class EmotionalImpactScoringTask : IVKScoringTask
{
    public VKPipelineSchedule Schedule => new(20);

    public Task<VKResult<VKScoringResult>> ExecuteAsync(VKScoringContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);
        if (context.Emotion is not null)
        {
            double emotionalScore = Math.Clamp(context.Emotion.Arousal * context.Emotion.Valence, 0.1, 1.0);
            return Task.FromResult(VKResult.Success(VKScoringResult.SuccessScore(emotionalScore)));
        }

        return Task.FromResult(VKResult.Success(VKScoringResult.SuccessScore(0.0)));
    }
}
