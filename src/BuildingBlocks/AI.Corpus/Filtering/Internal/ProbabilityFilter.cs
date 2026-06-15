using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that applies a probabilistic evaluation to decide whether to skip an entry.
/// Follows BB.01 / AP.03.
/// </summary>
internal sealed class ProbabilityFilter : IVKKnowledgeLifecycleFilter
{
    /// <inheritdoc />
    public int FilterOrder => 80;

    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> FilterAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        double threshold = entry.Lifecycle.Probability;
        if (threshold >= 1.0)
        {
            return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
        }

        if (threshold <= 0.0)
        {
            return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
        }

        // Generates a random number to evaluate against the threshold.
        double roll = Random.Shared.NextDouble();
        VKFilterVerdict verdict = roll <= threshold ? VKFilterVerdict.Keep : VKFilterVerdict.Reject;

        return Task.FromResult(VKResult.Success(verdict));
    }
}
