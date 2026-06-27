using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.VectorStore;
using VK.Blocks.VectorIngest;
using VK.Blocks.VectorIngest.Chunking.Internal;

namespace VK.Blocks.VectorIngest.Chunking.Internal; // [AP.03] Internal namespace

/// <summary>
/// Default semantic text chunker using embedding similarity.
/// </summary>
internal sealed class DefaultSemanticChunker(
    IVKEmbeddingsEngine embeddingsEngine,
    IVKGuidGenerator guidGenerator,
    IVKChunkingOptionsProvider optionsProvider) : IVKTextChunker // [AP.01] sealed default, [AP.03] internal scoping
{
    private readonly IVKEmbeddingsEngine _embeddingsEngine = VKGuard.NotNull(embeddingsEngine);
    private readonly IVKGuidGenerator _guidGenerator = VKGuard.NotNull(guidGenerator);
    private readonly IVKChunkingOptionsProvider _optionsProvider = VKGuard.NotNull(optionsProvider);

    private static readonly string[] SentenceSeparators = ["\r\n\r\n", "\n\n", "\r\n", "\n", "。", "？", "！", ".", "?", "!"];

    /// <inheritdoc />
    public async Task<VKResult<IReadOnlyList<VKChunk>>> ChunkAsync(
        string text,
        VKChunkingArgs args,
        CancellationToken cancellationToken = default) // [CS.03] Async + CancellationToken
    {
        VKGuard.NotNull(text); // [AP.01] VKGuard boundary
        VKGuard.NotNull(args);

        var options = _optionsProvider.GetOptions(args);

        if (string.IsNullOrWhiteSpace(text))
        {
            IReadOnlyList<VKChunk> emptyList = [];
            return VKResult.Success(emptyList);
        }

        // 1. Split text into raw sentences while preserving their positions/offsets
        var sentences = SplitIntoSentences(text);
        if (sentences.Count == 0)
        {
            IReadOnlyList<VKChunk> emptyList = [];
            return VKResult.Success(emptyList);
        }

        // 2. Generate embeddings for all sentences
        var embeddings = new List<VKVector>();
        foreach (var sentence in sentences)
        {
            var embedResult = await _embeddingsEngine.GenerateAsync(sentence.Content, cancellationToken).ConfigureAwait(false);
            if (embedResult.IsFailure)
            {
                return VKResult.Failure<IReadOnlyList<VKChunk>>(embedResult.Errors); // [CS.01] Result flow
            }
            embeddings.Add(embedResult.Value);
        }

        // 3. Compute similarity between consecutive sentences
        var similarities = new List<float>();
        for (int i = 0; i < sentences.Count - 1; i++)
        {
            similarities.Add(CalculateCosineSimilarity(embeddings[i], embeddings[i + 1]));
        }

        // 4. Group sentences based on similarity threshold and max size budget
        var chunks = new List<VKChunk>();
        var currentChunkSentences = new List<SentenceInfo>();
        var currentChunkLength = 0;
        var chunkIndex = 0;

        // Default threshold if not configured; normally we might want to configure this via options.
        // For now, we fall back to a reasonable 0.8f.
        const float threshold = 0.8f;

        for (int i = 0; i < sentences.Count; i++)
        {
            var sentence = sentences[i];

            // If adding this sentence exceeds options.ChunkSize and we already have some sentences in current chunk,
            // we must close the current chunk first.
            if (currentChunkLength + sentence.Content.Length > options.ChunkSize && currentChunkSentences.Count > 0)
            {
                chunks.Add(BuildChunk(currentChunkSentences, chunkIndex++));
                currentChunkSentences.Clear();
                currentChunkLength = 0;
            }

            currentChunkSentences.Add(sentence);
            currentChunkLength += sentence.Content.Length;

            // Check similarity with next sentence to decide if we should split
            if (i < sentences.Count - 1)
            {
                var similarity = similarities[i];
                if (similarity < threshold)
                {
                    // Split boundary reached
                    chunks.Add(BuildChunk(currentChunkSentences, chunkIndex++));
                    currentChunkSentences.Clear();
                    currentChunkLength = 0;
                }
            }
        }

        // Add final chunk if any sentences remain
        if (currentChunkSentences.Count > 0)
        {
            chunks.Add(BuildChunk(currentChunkSentences, chunkIndex++));
        }

        IReadOnlyList<VKChunk> result = chunks;
        return VKResult.Success(result);
    }

    private VKChunk BuildChunk(List<SentenceInfo> sentenceInfos, int index)
    {
        var sb = new StringBuilder();
        var startOffset = sentenceInfos[0].StartOffset;
        var endOffset = sentenceInfos[^1].EndOffset;

        foreach (var s in sentenceInfos)
        {
            sb.Append(s.Content);
        }

        return new VKChunk
        {
            Id = _guidGenerator.Create().ToString(), // [CS.06] Use IVKGuidGenerator
            Content = sb.ToString(),
            ChunkIndex = index,
            StartOffset = startOffset,
            EndOffset = endOffset
        };
    }

    private static List<SentenceInfo> SplitIntoSentences(string text)
    {
        var result = new List<SentenceInfo>();
        var currentOffset = 0;
        var textLength = text.Length;

        while (currentOffset < textLength)
        {
            // Find the earliest separator
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
                // No more separators found, add rest of the text as the final sentence
                var rest = text.Substring(currentOffset).Trim();
                if (rest.Length > 0)
                {
                    result.Add(new SentenceInfo(rest, currentOffset, textLength));
                }
                break;
            }

            var length = earliestIndex - currentOffset + chosenSeparatorLength;
            var sentence = text.Substring(currentOffset, length).Trim();
            if (sentence.Length > 0)
            {
                result.Add(new SentenceInfo(sentence, currentOffset, currentOffset + length));
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

    private sealed record SentenceInfo(string Content, int StartOffset, int EndOffset);
}
