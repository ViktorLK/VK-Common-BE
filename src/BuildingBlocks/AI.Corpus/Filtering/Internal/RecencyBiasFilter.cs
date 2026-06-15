using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that applies recency bias: the longer ago the entry was last injected,
/// the lower its probability of being selected.
/// Follows CS.01, CS.03, AP.01.
/// </summary>
internal sealed class RecencyBiasFilter : IVKKnowledgeLifecycleFilter
{
    private readonly VKFilteringOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="RecencyBiasFilter"/>.
    /// </summary>
    public RecencyBiasFilter(IOptions<VKFilteringOptions> options)
    {
        _options = VKGuard.NotNull(options?.Value);
    }

    /// <inheritdoc />
    public int FilterOrder => 170;

    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> FilterAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        string idValue = entry.Knowledge.Id.Value.ToString();

        // If it was never injected in this session, we don't apply recency bias decay.
        if (context.LastInjectedTurns.TryGetValue(idValue, out int lastTurn))
        {
            int gap = context.CurrentTurn - lastTurn;
            if (gap > 0)
            {
                // Simple hyperbola decay: 1.0 / (1.0 + (gap * RecencyDecayFactor))
                double decay = 1.0 / (1.0 + (gap * _options.RecencyDecayFactor));
                double targetProbability = decay;

                double roll = Random.Shared.NextDouble();
                if (roll > targetProbability)
                {
                    return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
                }
            }
        }

        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}
