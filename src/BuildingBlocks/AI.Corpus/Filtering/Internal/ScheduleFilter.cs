using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that checks if the current turn is within the scheduled range for an entry.
/// Follows BB.01 / AP.03.
/// </summary>
internal sealed class ScheduleFilter : IVKKnowledgeLifecycleFilter
{
    /// <inheritdoc />
    public int FilterOrder => 40;

    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> FilterAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        if (entry.Lifecycle.StartTurn is { } startTurn && context.CurrentTurn < startTurn)
        {
            // Current turn is before scheduled start turn: filter out
            return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
        }

        if (entry.Lifecycle.EndTurn is { } endTurn && context.CurrentTurn > endTurn)
        {
            // Current turn is after scheduled end turn: filter out
            return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}
