using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that gates entry injection based on affection and anger levels.
/// Follows CS.01, CS.03, AP.01.
/// </summary>
internal sealed class EmotionGatedFilter : IVKKnowledgeLifecycleFilter
{
    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> EvaluateAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        // Check MinAffection
        if (entry.Lifecycle.MinAffection is { } minAffection)
        {
            int affection = context.StateValues.TryGetValue("affection", out int val) ? val : 0;
            if (affection < minAffection)
            {
                return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
            }
        }

        // Check MaxAnger
        if (entry.Lifecycle.MaxAnger is { } maxAnger)
        {
            int anger = context.StateValues.TryGetValue("anger", out int val) ? val : 0;
            if (anger > maxAnger)
            {
                return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
            }
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}



