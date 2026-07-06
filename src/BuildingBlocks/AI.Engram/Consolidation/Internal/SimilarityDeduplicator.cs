using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.VectorStore;

namespace VK.Blocks.AI.Engram.Consolidation;

internal sealed class SimilarityDeduplicator
{
    private readonly IVKEmbeddingsEngine _embeddingsEngine;
    private readonly IVKChatEngine _chatEngine;

    public SimilarityDeduplicator(IVKEmbeddingsEngine embeddingsEngine, IVKChatEngine chatEngine)
    {
        _embeddingsEngine = VKGuard.NotNull(embeddingsEngine);
        _chatEngine = VKGuard.NotNull(chatEngine);
    }

    public async Task<VKResult<List<VKMemoryEntry>>> DeduplicateAsync(
        List<VKMemoryEntry> candidates,
        double similarityThreshold,
        double dropLowerThreshold,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(candidates);

        if (candidates.Count <= 1)
        {
            return VKResult.Success(candidates);
        }

        // 1. Generate embeddings for all candidate contents in parallel
        var tasks = candidates.Select(c => _embeddingsEngine.GenerateAsync(c.Content, cancellationToken)).ToList();
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        var vectors = new List<VKVector>();
        var errors = new List<VKError>();
        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                errors.AddRange(result.Errors);
            }
            else
            {
                vectors.Add(result.Value);
            }
        }

        if (errors.Count > 0)
        {
            return VKResult.Failure<List<VKMemoryEntry>>(errors);
        }

        var list = new List<(VKMemoryEntry Entry, ReadOnlyMemory<float> Vector)>();
        for (int i = 0; i < candidates.Count; i++)
        {
            list.Add((candidates[i], vectors[i].Values));
        }

        var consolidated = new List<VKMemoryEntry>();
        var skipIndices = new HashSet<int>();

        for (int i = 0; i < list.Count; i++)
        {
            if (skipIndices.Contains(i))
                continue;

            var current = list[i];
            var activeEntry = current.Entry;

            for (int j = i + 1; j < list.Count; j++)
            {
                if (skipIndices.Contains(j))
                    continue;

                var other = list[j];
                double similarity = CalculateCosineSimilarity(current.Vector.Span, other.Vector.Span);

                if (similarity >= dropLowerThreshold)
                {
                    // Drop lower score (DropLower)
                    if (other.Entry.Importance < activeEntry.Importance)
                    {
                        skipIndices.Add(j);
                    }
                    else
                    {
                        activeEntry = other.Entry;
                        skipIndices.Add(j);
                    }
                }
                else if (similarity >= similarityThreshold)
                {
                    // Merge via summary (MergeViaSummary) using LLM
                    var mergeResult = await MergeViaSummaryAsync(activeEntry.Content, other.Entry.Content, cancellationToken).ConfigureAwait(false);
                    if (mergeResult.IsSuccess)
                    {
                        activeEntry = activeEntry with
                        {
                            Content = mergeResult.Value,
                            Importance = Math.Max(activeEntry.Importance, other.Entry.Importance)
                        };
                        skipIndices.Add(j);
                    }
                }
            }

            consolidated.Add(activeEntry);
        }

        return VKResult.Success(consolidated);
    }

    private static double CalculateCosineSimilarity(ReadOnlySpan<float> vecA, ReadOnlySpan<float> vecB)
    {
        if (vecA.Length != vecB.Length || vecA.Length == 0)
            return 0;

        double dotProduct = 0;
        double normA = 0;
        double normB = 0;

        for (int i = 0; i < vecA.Length; i++)
        {
            dotProduct += vecA[i] * vecB[i];
            normA += vecA[i] * vecA[i];
            normB += vecB[i] * vecB[i];
        }

        if (normA == 0 || normB == 0)
            return 0;

        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    private async Task<VKResult<string>> MergeViaSummaryAsync(string text1, string text2, CancellationToken cancellationToken)
    {
        string prompt = "You are a memory deduplicator. Merge the following two highly similar memory segments into a single, cohesive, non-redundant statement.\n\n" +
                        $"Memory A:\n{text1}\n\n" +
                        $"Memory B:\n{text2}\n\n" +
                        "Output the merged memory cleanly. Do not include markdown wrappers, introduction, or explanation.";

        var messages = new[] { VKChatMessage.FromText(VKChatRole.User, prompt) };

        try
        {
            var result = await _chatEngine.SendAsync(messages, null, cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                return VKResult.Failure<string>(result.Errors);
            }
            return VKResult.Success(result.Value.Message.Content ?? string.Empty);
        }
        catch (Exception ex)
        {
            return VKResult.Failure<string>(new VKError("AI.Engram.Consolidation.MergeViaSummaryError", ex.Message));
        }
    }
}
