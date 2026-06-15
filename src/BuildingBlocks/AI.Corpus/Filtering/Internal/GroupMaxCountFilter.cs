using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that checks the session-wide injection count limit for a group of entries.
/// Follows BB.01 / AP.03.
/// </summary>
internal sealed class GroupMaxCountFilter : IVKKnowledgeLifecycleFilter
{
    /// <inheritdoc />
    public int FilterOrder => 120;

    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> FilterAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        string? groupId = entry.Lifecycle.GroupId;
        if (string.IsNullOrWhiteSpace(groupId))
        {
            return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
        }

        if (entry.Lifecycle.MaxCount is { } maxCount)
        {
            string groupKey = $"group:{groupId}";
            if (context.UsageCounts.TryGetValue(groupKey, out int currentCount) && currentCount >= maxCount)
            {
                // Maximum count for the group has been reached: filter out
                return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
            }
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}
