using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that gates entry injection based on the user segment.
/// Follows CS.01, CS.03, AP.01.
/// </summary>
internal sealed class UserSegmentFilter : IVKKnowledgeLifecycleFilter
{
    /// <inheritdoc />
    public int FilterOrder => 20;

    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> FilterAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        string? targetSegment = entry.Lifecycle.UserSegment;
        if (!string.IsNullOrWhiteSpace(targetSegment))
        {
            if (!string.Equals(targetSegment, context.UserSegment, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
            }
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}
