using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch.Fusion.Internal;

/// <summary>
/// Implements Weighted Score Fusion with Min-Max normalization per run.
/// </summary>
internal sealed class WeightedScoreFusion : IVKScoreFusion
{
    private readonly float[] _weights;

    public WeightedScoreFusion(IEnumerable<float>? weights = null)
    {
        _weights = weights?.ToArray() ?? [0.5f, 0.5f];
    }

    public VKResult<IReadOnlyList<VKSearchResult>> Fuse(
        IReadOnlyList<IReadOnlyList<VKFusionCandidate>> runs,
        int topK)
    {
        // [RuleID: AP.01]
        VKGuard.NotNull(runs);

        var scores = new Dictionary<string, (VKDocumentChunk Chunk, float Score)>();

        for (var runIndex = 0; runIndex < runs.Count; runIndex++)
        {
            var run = runs[runIndex];
            if (run is null || run.Count == 0) continue;

            var weight = runIndex < _weights.Length ? _weights[runIndex] : 1.0f;

            var maxScore = run.Max(c => c.Score);
            var minScore = run.Min(c => c.Score);
            var range = maxScore - minScore;

            foreach (var candidate in run)
            {
                if (candidate?.Chunk is null) continue;

                var normalizedScore = range > 1e-6f 
                    ? (candidate.Score - minScore) / range 
                    : 1.0f;

                var weightedContribution = normalizedScore * weight;

                var key = candidate.Chunk.Id;
                if (scores.TryGetValue(key, out var existing))
                {
                    scores[key] = (existing.Chunk, existing.Score + weightedContribution);
                }
                else
                {
                    scores[key] = (candidate.Chunk, weightedContribution);
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
