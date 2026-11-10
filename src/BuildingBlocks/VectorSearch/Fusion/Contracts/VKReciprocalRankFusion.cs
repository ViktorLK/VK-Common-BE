using System;
using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Pure, I/O-free Reciprocal Rank Fusion (RRF) mathematical algorithm [ARCH-03 Core Purity].
/// Fuses multiple ranked result lists using the formula: Score = Sum(1.0 / (k + rank)).
/// </summary>
public static class VKReciprocalRankFusion
{
    /// <summary>
    /// The default ranking constant k (standard industry default: 60).
    /// </summary>
    public const float DefaultK = 60.0f;

    /// <summary>
    /// Computes fused reciprocal rank scores for arbitrary keys across multiple ranking runs.
    /// </summary>
    /// <typeparam name="TKey">The identity type of the candidate item.</typeparam>
    /// <param name="runs">List of ranking runs, each containing keys ordered by relevance.</param>
    /// <param name="topK">The maximum number of items to return.</param>
    /// <param name="k">The smoothing constant k (must be greater than 0).</param>
    /// <param name="comparer">Optional equality comparer for keys.</param>
    /// <returns>A Result containing the fused, sorted list of items with their scores and final ranks.</returns>
    public static VKResult<IReadOnlyList<VKFusionItem<TKey>>> Compute<TKey>(
        IReadOnlyList<IReadOnlyList<TKey>> runs,
        int topK,
        float k = DefaultK,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        VKGuard.NotNull(runs); // [RuleID: AP.01]

        if (topK <= 0)
        {
            return VKResult.Success<IReadOnlyList<VKFusionItem<TKey>>>([]);
        }

        if (k <= 0f)
        {
            return VKResult.Failure<IReadOnlyList<VKFusionItem<TKey>>>(VKError.Validation(
                "VectorSearch.Fusion.InvalidK",
                "RRF constant k must be greater than zero."));
        }

        var totalCandidates = 0;
        for (var i = 0; i < runs.Count; i++)
        {
            if (runs[i] is not null)
            {
                totalCandidates += runs[i].Count;
            }
        }

        if (totalCandidates == 0)
        {
            return VKResult.Success<IReadOnlyList<VKFusionItem<TKey>>>([]);
        }

        var scores = new Dictionary<TKey, float>(totalCandidates, comparer);

        for (var runIdx = 0; runIdx < runs.Count; runIdx++)
        {
            var run = runs[runIdx];
            if (run is null)
                continue;

            for (var rankIdx = 0; rankIdx < run.Count; rankIdx++)
            {
                var key = run[rankIdx];
                if (key is null)
                    continue;

                var rank = rankIdx + 1;
                var contribution = 1.0f / (k + rank);

                if (scores.TryGetValue(key, out var currentScore))
                {
                    scores[key] = currentScore + contribution;
                }
                else
                {
                    scores[key] = contribution;
                }
            }
        }

        var sorted = scores
            .OrderByDescending(kv => kv.Value)
            .Take(topK)
            .Select((kv, idx) => new VKFusionItem<TKey>(kv.Key, kv.Value, idx + 1))
            .ToList();

        return VKResult.Success<IReadOnlyList<VKFusionItem<TKey>>>(sorted);
    }
}

/// <summary>
/// Represents an item evaluated and ranked by Reciprocal Rank Fusion.
/// </summary>
/// <typeparam name="TKey">The identity type of the item.</typeparam>
/// <param name="Key">The item key or identity.</param>
/// <param name="Score">The accumulated RRF score.</param>
/// <param name="FinalRank">The 1-based final rank position.</param>
public readonly record struct VKFusionItem<TKey>(TKey Key, float Score, int FinalRank)
    where TKey : notnull;

