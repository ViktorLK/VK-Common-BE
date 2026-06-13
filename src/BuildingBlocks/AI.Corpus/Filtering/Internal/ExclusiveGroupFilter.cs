using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that enforces exclusive grouping: only one entry per GroupId is allowed in the same turn.
/// Assumes candidate entries are sorted by ExclusiveWeight descending (highest weight first).
/// Follows CS.01, CS.03, AP.01.
/// </summary>
internal sealed class ExclusiveGroupFilter : IVKKnowledgeLifecycleFilter
{
    private readonly HashSet<string> _resolvedGroups = new(System.StringComparer.OrdinalIgnoreCase);

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
            if (_resolvedGroups.Contains(groupId))
            {
                // Another entry in this exclusive group with higher/equal weight has already been kept
                return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
            }

            _resolvedGroups.Add(groupId);
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}



