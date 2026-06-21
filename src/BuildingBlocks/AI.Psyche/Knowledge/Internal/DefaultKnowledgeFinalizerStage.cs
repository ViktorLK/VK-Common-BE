using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Knowledge.Internal;

/// <summary>
/// A pipeline stage that finalizes knowledge candidates by adding them to context fragments before weaving.
/// Implements <see cref="IVKPsycheBeforePipelineStage"/>.
/// Follows BB.01 / AP.03.
/// </summary>
internal sealed class DefaultKnowledgeFinalizerStage : IVKPsycheBeforePipelineStage
{
    /// <inheritdoc />
    public VKPipelineStageSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheKnowledgeFinalizer;

    /// <inheritdoc />
    public bool IsActive => true;

    /// <inheritdoc />
    public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        var state = context.State<VKKnowledgeCandidatesState>();
        if (state != null)
        {
            for (int i = 0; i < state.Candidates.Count; i++)
            {
                VKKnowledgeEntry entry = state.Candidates[i];
                context.AddFragment(new VKPromptFragment
                {
                    TierType = VKPromptTierType.Knowledge,
                    Metadata = entry,
                    Segment = entry.Segment
                });
            }
        }

        return Task.FromResult(VKResult.Success());
    }
}
