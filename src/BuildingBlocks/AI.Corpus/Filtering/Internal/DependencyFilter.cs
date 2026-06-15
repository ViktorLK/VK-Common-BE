using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that skips an entry if its parent dependency (identified by DependencyId) has not been injected.
/// Follows BB.01 / AP.03.
/// </summary>
internal sealed class DependencyFilter : IVKKnowledgeLifecycleFilter
{
    /// <inheritdoc />
    public int FilterOrder => 60;

    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> FilterAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        string? dependencyId = entry.Lifecycle.DependencyId;
        if (!string.IsNullOrWhiteSpace(dependencyId))
        {
            // The parent dependency must have been injected in the current context or session history
            bool hasParentPassed = context.InjectedTags.Contains(dependencyId) ||
                                   (context.UsageCounts.TryGetValue(dependencyId, out int count) && count > 0);

            if (!hasParentPassed)
            {
                // Dependency entry has not passed: filter out
                return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
            }
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}
