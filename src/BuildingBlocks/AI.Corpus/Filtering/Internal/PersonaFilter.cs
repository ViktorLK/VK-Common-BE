using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that gates entry injection based on whether it matches the current persona.
/// Follows CS.01, CS.03, AP.01.
/// </summary>
internal sealed class PersonaFilter : IVKKnowledgeLifecycleFilter
{
    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> EvaluateAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        string? targetPersonaId = entry.Lifecycle.TargetPersonaId;
        if (!string.IsNullOrWhiteSpace(targetPersonaId))
        {
            if (!string.Equals(targetPersonaId, context.PersonaId, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
            }
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}



