using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that gates entry injection based on language culture matching.
/// Follows CS.01, CS.03, AP.01.
/// </summary>
internal sealed class LanguageFilter : IVKKnowledgeLifecycleFilter
{
    /// <inheritdoc />
    public int FilterOrder => 25;

    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> FilterAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        string? targetLanguage = entry.Lifecycle.Language;
        if (!string.IsNullOrWhiteSpace(targetLanguage) && !string.IsNullOrWhiteSpace(context.Language))
        {
            if (!string.Equals(targetLanguage, context.Language, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
            }
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}
