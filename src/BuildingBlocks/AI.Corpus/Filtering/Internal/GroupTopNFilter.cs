using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that enforces a maximum count of injected entries per group in a single turn.
/// Assumes candidate entries are sorted by ExclusiveWeight descending (highest weight first).
/// Follows CS.01, CS.03, AP.01.
/// </summary>
internal sealed class GroupTopNFilter : IVKKnowledgeLifecycleFilter
{
    private readonly Dictionary<string, int> _resolvedGroupCounts = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> EvaluateAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        string? groupId = entry.Lifecycle.GroupId;
        if (!string.IsNullOrWhiteSpace(groupId))
        {
            int maxCountPerTurn = entry.Lifecycle.MaxCountPerTurn ?? 1;
            _resolvedGroupCounts.TryGetValue(groupId, out int currentCount);
            if (currentCount >= maxCountPerTurn)
            {
                // Limit reached for this turn: filter out subsequent lower-weight entries
                return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
            }

            _resolvedGroupCounts[groupId] = currentCount + 1;
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}
