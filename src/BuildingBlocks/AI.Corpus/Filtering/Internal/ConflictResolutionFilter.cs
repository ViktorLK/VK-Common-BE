using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that resolves conflicts by ensuring only one entry per conflict group is injected in the same turn.
/// Assumes higher-priority entries are evaluated first.
/// Follows CS.01, CS.03, AP.01.
/// </summary>
internal sealed class ConflictResolutionFilter : IVKKnowledgeLifecycleFilter
{
    private readonly HashSet<string> _resolvedConflictGroups = new(System.StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> EvaluateAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        string? conflictGroupId = entry.Lifecycle.ConflictGroupId;
        if (!string.IsNullOrWhiteSpace(conflictGroupId))
        {
            if (_resolvedConflictGroups.Contains(conflictGroupId))
            {
                // An entry in this conflict group has already been kept (higher priority first)
                return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
            }

            _resolvedConflictGroups.Add(conflictGroupId);
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}
