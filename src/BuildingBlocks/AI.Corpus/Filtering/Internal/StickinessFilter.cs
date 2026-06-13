using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that checks if an entry is within its sticky turns duration and forces it to pass.
/// Follows CS.01, CS.03, AP.01.
/// </summary>
internal sealed class StickinessFilter : IVKKnowledgeLifecycleFilter
{
    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> EvaluateAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        if (entry.Lifecycle.StickyTurns is { } stickyTurns)
        {
            if (stickyTurns == -1)
            {
                // Permanent/Anchor stickiness
                return Task.FromResult(VKResult.Success(VKFilterVerdict.ForceKeep));
            }

            if (stickyTurns > 0)
            {
                string idValue = entry.Knowledge.Id.Value.ToString();
                if (context.LastInjectedTurns.TryGetValue(idValue, out int lastTurn))
                {
                    if (context.CurrentTurn - lastTurn < stickyTurns)
                    {
                        // Within sticky turns duration: must be kept
                        return Task.FromResult(VKResult.Success(VKFilterVerdict.ForceKeep));
                    }
                }
            }
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}



