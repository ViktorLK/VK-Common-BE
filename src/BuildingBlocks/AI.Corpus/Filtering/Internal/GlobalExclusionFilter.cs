using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that skips an entry if a mutually exclusive entry (identified by ExclusionTag) has already been injected in global context.
/// Follows BB.01 / AP.03.
/// </summary>
internal sealed class GlobalExclusionFilter : IVKKnowledgeLifecycleFilter
{
    /// <inheritdoc />
    public int FilterOrder => 130;

    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> FilterAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        string? exclusionTag = entry.Lifecycle.ExclusionTag;
        if (!string.IsNullOrWhiteSpace(exclusionTag))
        {
            if (context.InjectedTags.Contains(exclusionTag))
            {
                // Exclusive entry has already been injected in the current turn/context: filter out
                return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
            }
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}
