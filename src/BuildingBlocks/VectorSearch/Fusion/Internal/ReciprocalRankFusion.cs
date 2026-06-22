using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch.Fusion.Internal;

/// <summary>
/// Implements Reciprocal Rank Fusion (RRF) algorithm.
/// </summary>
internal sealed class ReciprocalRankFusion : IVKScoreFusion
{
    private const float K = 60f;

    public VKResult<IReadOnlyList<VKSearchResult>> Fuse(
        IReadOnlyList<IReadOnlyList<VKFusionCandidate>> runs,
        int topK)
    {
        // [RuleID: AP.01]
        VKGuard.NotNull(runs);

        var initialCapacity = 0;
        var nonNullRuns = 0;
        IReadOnlyList<VKFusionCandidate>? singleRun = null;

        for (var i = 0; i < runs.Count; i++)
        {
            var run = runs[i];
            if (run is not null && run.Count > 0)
            {
                initialCapacity += run.Count;
                nonNullRuns++;
                singleRun = run;
            }
        }

        // Fast path: Empty input
        if (initialCapacity == 0)
        {
            return VKResult.Success<IReadOnlyList<VKSearchResult>>([]);
        }

        // Fast path: Only one non-empty run
        if (nonNullRuns == 1 && singleRun is not null)
        {
            var limit = System.Math.Min(singleRun.Count, topK);
            var fastResults = new List<VKSearchResult>(limit);
            for (var i = 0; i < limit; i++)
            {
                var candidate = singleRun[i];
                if (candidate?.Chunk is null) continue;

                var rank = candidate.Rank > 0 ? candidate.Rank : i + 1;
                fastResults.Add(new VKSearchResult
                {
                    Document = new VKDocument
                    {
                        Id = candidate.Chunk.DocumentId,
                        Content = candidate.Chunk.Content,
                        Metadata = candidate.Chunk.Metadata.Properties.TryGetValue("DocumentMetadata", out var docMeta) ? docMeta : string.Empty
                    },
                    Score = 1.0f / (K + rank)
                });
            }
            return VKResult.Success<IReadOnlyList<VKSearchResult>>(fastResults);
        }

        var scores = new Dictionary<string, (VKDocumentChunk Chunk, float Score)>(initialCapacity);

        for (var runIndex = 0; runIndex < runs.Count; runIndex++)
        {
            var run = runs[runIndex];
            if (run is null) continue;

            for (var i = 0; i < run.Count; i++)
            {
                var candidate = run[i];
                if (candidate?.Chunk is null) continue;

                var rank = candidate.Rank > 0 ? candidate.Rank : i + 1;
                var scoreContribution = 1.0f / (K + rank);

                var key = candidate.Chunk.Id;
                if (scores.TryGetValue(key, out var existing))
                {
                    scores[key] = (existing.Chunk, existing.Score + scoreContribution);
                }
                else
                {
                    scores[key] = (candidate.Chunk, scoreContribution);
                }
            }
        }

        var results = scores.Values
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Select(x => new VKSearchResult
            {
                Document = new VKDocument
                {
                    Id = x.Chunk.DocumentId,
                    Content = x.Chunk.Content,
                    Metadata = x.Chunk.Metadata.Properties.TryGetValue("DocumentMetadata", out var docMeta) ? docMeta : string.Empty
                },
                Score = x.Score
            })
            .ToList();

        return VKResult.Success<IReadOnlyList<VKSearchResult>>(results);
    }
}
