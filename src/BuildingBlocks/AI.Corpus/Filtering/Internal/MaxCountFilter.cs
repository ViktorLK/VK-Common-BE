using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that checks the session-wide individual injection limit for an entry and skips it if exceeded.
/// Follows BB.01 / AP.03.
/// </summary>
internal sealed class MaxCountFilter : IVKKnowledgeLifecycleFilter
{
    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> EvaluateAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        // Individual max count applies only when not part of a coordinated group check
        if (entry.Lifecycle.MaxCount is { } maxCount && string.IsNullOrEmpty(entry.Lifecycle.GroupId))
        {
            string idValue = entry.Knowledge.Id.Value.ToString();
            if (context.UsageCounts.TryGetValue(idValue, out int count) && count >= maxCount)
            {
                // Max session-wide count reached: filter out
                return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
            }
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}



