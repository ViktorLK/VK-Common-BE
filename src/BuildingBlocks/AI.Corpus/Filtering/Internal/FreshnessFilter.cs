using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that ignores entries that have expired based on ExpiresAt.
/// Follows CS.01, CS.03, CS.06, AP.01.
/// </summary>
internal sealed class FreshnessFilter : IVKKnowledgeLifecycleFilter
{
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="FreshnessFilter"/>.
    /// </summary>
    public FreshnessFilter(TimeProvider timeProvider)
    {
        _timeProvider = VKGuard.NotNull(timeProvider);
    }

    /// <inheritdoc />
    public int FilterOrder => 30;

    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> FilterAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        if (entry.Lifecycle.ExpiresAt is { } expiresAt)
        {
            if (_timeProvider.GetUtcNow() > expiresAt)
            {
                return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
            }
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}
