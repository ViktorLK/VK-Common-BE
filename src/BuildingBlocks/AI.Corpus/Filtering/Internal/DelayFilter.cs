using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that enforces delayed activation turns after an entry has been triggered.
/// Expects the trigger turn to be recorded in context.StateValues under the key "trigger_turn:{entryId}".
/// Follows CS.01, CS.03, AP.01.
/// </summary>
internal sealed class DelayFilter : IVKKnowledgeLifecycleFilter
{
    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> EvaluateAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        int delayTurns = entry.Lifecycle.DelayTurns;
        if (delayTurns > 0)
        {
            string idValue = entry.Knowledge.Id.Value.ToString();
            string stateKey = $"trigger_turn:{idValue}";

            if (context.StateValues.TryGetValue(stateKey, out int triggeredTurn))
            {
                int elapsed = context.CurrentTurn - triggeredTurn;
                if (elapsed < delayTurns)
                {
                    // Delay period not yet satisfied: filter out
                    return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
                }
            }
            else
            {
                // Has delay but never triggered: filter out
                return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
            }
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}
