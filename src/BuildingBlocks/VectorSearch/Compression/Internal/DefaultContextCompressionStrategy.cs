using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.VectorStore;

namespace VK.Blocks.VectorSearch.Compression.Internal;

/// <summary>
/// Default contextual compression strategy that uses sentence-level embedding similarity to prune irrelevant content.
/// </summary>
internal sealed class DefaultContextCompressionStrategy : IVKContextCompressionStrategy
{
    private readonly IVKEmbeddingsEngine _embeddingsEngine;
    private readonly VKContextCompressionOptions _options;

    private static readonly string[] SentenceSeparators = ["\r\n\r\n", "\n\n", "\r\n", "\n", "。", "？", "！", ".", "?", "!"];

    public DefaultContextCompressionStrategy(
        IVKEmbeddingsEngine embeddingsEngine,
        IOptions<VKContextCompressionOptions> options)
    {
        _embeddingsEngine = VKGuard.NotNull(embeddingsEngine);
        _options = VKGuard.NotNull(options?.Value);
    }

    public async Task<VKResult<VKSearchResult[]>> CompressContextAsync(
        VKSearchResult[] results,
        string query,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(results);
        VKGuard.NotNullOrWhiteSpace(query);

        if (results.Length == 0)
        {
            return VKResult.Success(results);
        }

        // 1. Generate embedding for the query
        var queryEmbedResult = await _embeddingsEngine.GenerateAsync(query, cancellationToken).ConfigureAwait(false);
        if (queryEmbedResult.IsFailure)
        {
            return VKResult.Failure<VKSearchResult[]>(queryEmbedResult.Errors);
        }
        var queryVector = queryEmbedResult.Value;

        var compressedResults = new List<VKSearchResult>();

        foreach (var result in results)
        {
            if (string.IsNullOrWhiteSpace(result.Document.Content))
            {
                compressedResults.Add(result);
                continue;
            }

            // 2. Split content into sentences
            var sentences = SplitIntoSentences(result.Document.Content);
            if (sentences.Count == 0)
            {
                compressedResults.Add(result);
                continue;
            }

            // 3. Compute embeddings and similarity score for each sentence
            var sentenceScores = new List<SentenceScoreInfo>();
            for (int i = 0; i < sentences.Count; i++)
            {
                var sentenceText = sentences[i];
                var sentenceEmbedResult = await _embeddingsEngine.GenerateAsync(sentenceText, cancellationToken).ConfigureAwait(false);
                if (sentenceEmbedResult.IsFailure)
                {
                    return VKResult.Failure<VKSearchResult[]>(sentenceEmbedResult.Errors);
                }

                var similarity = CalculateCosineSimilarity(queryVector, sentenceEmbedResult.Value);
                sentenceScores.Add(new SentenceScoreInfo(sentenceText, i, similarity));
            }

            // 4. Filter by threshold
            var selectedSentences = sentenceScores
                .Where(s => s.Similarity >= _options.SimilarityThreshold)
                .ToList();

            // 5. Fallback if kept count is below MinSentences
            if (selectedSentences.Count < _options.MinSentences)
            {
                selectedSentences = sentenceScores
                    .OrderByDescending(s => s.Similarity)
                    .Take(_options.MinSentences)
                    .ToList();
            }

            // 6. Restore original order of appearance
            var orderedSentences = selectedSentences
                .OrderBy(s => s.Index)
                .Select(s => s.Content)
                .ToList();

            // 7. Join sentences back
            var compressedContent = string.Join(" ", orderedSentences);

            compressedResults.Add(result with
            {
                Document = result.Document with { Content = compressedContent }
            });
        }

        return VKResult.Success(compressedResults.ToArray());
    }

    private static List<string> SplitIntoSentences(string text)
    {
        var result = new List<string>();
        var currentOffset = 0;
        var textLength = text.Length;

        while (currentOffset < textLength)
        {
            var earliestIndex = textLength;
            var chosenSeparatorLength = 0;

            foreach (var sep in SentenceSeparators)
            {
                var idx = text.IndexOf(sep, currentOffset, StringComparison.Ordinal);
                if (idx != -1 && idx < earliestIndex)
                {
                    earliestIndex = idx;
                    chosenSeparatorLength = sep.Length;
                }
            }

            if (earliestIndex == textLength)
            {
                var rest = text.Substring(currentOffset).Trim();
                if (rest.Length > 0)
                {
                    result.Add(rest);
                }
                break;
            }

            var length = earliestIndex - currentOffset + chosenSeparatorLength;
            var sentence = text.Substring(currentOffset, length).Trim();
            if (sentence.Length > 0)
            {
                result.Add(sentence);
            }

            currentOffset = earliestIndex + chosenSeparatorLength;
        }

        return result;
    }

    private static float CalculateCosineSimilarity(VKVector a, VKVector b)
    {
        var va = a.Values.Span;
        var vb = b.Values.Span;
        if (va.Length != vb.Length) return 0f;

        double dotProduct = 0;
        double normA = 0;
        double normB = 0;

        for (int i = 0; i < va.Length; i++)
        {
            dotProduct += va[i] * vb[i];
            normA += va[i] * va[i];
            normB += vb[i] * vb[i];
        }

        if (normA == 0 || normB == 0) return 0f;
        return (float)(dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB)));
    }

    private sealed record SentenceScoreInfo(string Content, int Index, float Similarity);
}
