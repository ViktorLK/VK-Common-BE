using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.VectorIngest;
using VK.Blocks.VectorIngest.Chunking.Internal;

namespace VK.Blocks.VectorIngest.Chunking.Internal; // [AP.03] Internal namespace

/// <summary>
/// Default recursive text chunker trying to maintain semantic boundaries.
/// </summary>
internal sealed class DefaultRecursiveChunker(
    IVKGuidGenerator guidGenerator,
    IVKChunkingOptionsProvider optionsProvider) : IVKTextChunker // [AP.01] sealed default, [AP.03] internal scoping
{
    private readonly IVKGuidGenerator _guidGenerator = VKGuard.NotNull(guidGenerator);
    private readonly IVKChunkingOptionsProvider _optionsProvider = VKGuard.NotNull(optionsProvider);
    private static readonly string[] DefaultSeparators = ["\r\n\r\n", "\n\n", "\r\n", "\n", "。", "？", "！", ".", "?", "!", "；", ";", " ", ""];

    /// <inheritdoc />
    public Task<VKResult<IReadOnlyList<VKChunk>>> ChunkAsync(
        string text,
        VKChunkingArgs args,
        CancellationToken cancellationToken = default) // [CS.03] Async + CancellationToken
    {
        VKGuard.NotNull(text); // [AP.01] VKGuard boundary
        VKGuard.NotNull(args);

        var options = _optionsProvider.GetOptions(args);

        if (options.ChunkSize <= 0)
        {
            return Task.FromResult(VKResult.Failure<IReadOnlyList<VKChunk>>(VKError.Validation(
                "AI.Ingest.Chunking.InvalidSize",
                "ChunkSize must be greater than zero."))); // [CS.01] Predefined error constant
        }

        if (options.ChunkOverlap < 0 || options.ChunkOverlap >= options.ChunkSize)
        {
            return Task.FromResult(VKResult.Failure<IReadOnlyList<VKChunk>>(VKError.Validation(
                "AI.Ingest.Chunking.InvalidOverlap",
                "ChunkOverlap must be non-negative and less than ChunkSize."))); // [CS.01] Predefined error constant
        }

        var chunks = new List<VKChunk>();
        SplitText(text, 0, DefaultSeparators, options.ChunkSize, options.ChunkOverlap, chunks);

        // Normalize indices and assign Guid IDs
        var finalChunks = new List<VKChunk>(chunks.Count);
        for (var i = 0; i < chunks.Count; i++)
        {
            finalChunks.Add(chunks[i] with 
            { 
                Id = _guidGenerator.Create().ToString(), // [CS.06] Use IVKGuidGenerator
                ChunkIndex = i 
            });
        }

        IReadOnlyList<VKChunk> result = finalChunks;
        return Task.FromResult(VKResult.Success(result));
    }

    private static void SplitText(
        string text,
        int baseOffset,
        string[] separators,
        int maxSize,
        int overlap,
        List<VKChunk> result)
    {
        if (text.Length <= maxSize)
        {
            result.Add(new VKChunk
            {
                Id = "", // Assigned during normalization
                Content = text,
                ChunkIndex = 0,
                StartOffset = baseOffset,
                EndOffset = baseOffset + text.Length
            });
            return;
        }

        var separator = "";
        var nextSeparators = separators;
        for (var i = 0; i < separators.Length; i++)
        {
            if (separators[i] == "" || text.Contains(separators[i]))
            {
                separator = separators[i];
                nextSeparators = separators[(i + 1)..];
                break;
            }
        }

        var splits = SplitOnSeparator(text, separator);
        var currentDoc = new StringBuilder();
        var currentOffset = 0;

        foreach (var part in splits)
        {
            if (part.Length == 0) continue;

            var partIndex = text.IndexOf(part, currentOffset, StringComparison.Ordinal);
            if (partIndex == -1)
            {
                partIndex = currentOffset;
            }

            if (currentDoc.Length + part.Length <= maxSize)
            {
                currentDoc.Append(part);
                currentOffset = partIndex + part.Length;
            }
            else
            {
                if (currentDoc.Length > 0)
                {
                    var docText = currentDoc.ToString().Trim();
                    if (docText.Length > 0)
                    {
                        var docStart = text.IndexOf(docText, StringComparison.Ordinal);
                        if (docStart == -1) docStart = 0;
                        result.Add(new VKChunk
                        {
                            Id = "", // Assigned during normalization
                            Content = docText,
                            ChunkIndex = 0,
                            StartOffset = baseOffset + docStart,
                            EndOffset = baseOffset + docStart + docText.Length
                        });
                    }

                    // Compute overlap
                    var overlapText = GetOverlapText(currentDoc.ToString(), overlap);
                    currentDoc.Clear();
                    currentDoc.Append(overlapText);
                }

                if (part.Length > maxSize)
                {
                    SplitText(part, baseOffset + partIndex, nextSeparators, maxSize, overlap, result);
                    currentOffset = partIndex + part.Length;
                }
                else
                {
                    currentDoc.Append(part);
                    currentOffset = partIndex + part.Length;
                }
            }
        }

        if (currentDoc.Length > 0)
        {
            var remaining = currentDoc.ToString().Trim();
            if (remaining.Length > 0)
            {
                var docStart = text.IndexOf(remaining, StringComparison.Ordinal);
                if (docStart == -1) docStart = 0;
                result.Add(new VKChunk
                {
                    Id = "", // Assigned during normalization
                    Content = remaining,
                    ChunkIndex = 0,
                    StartOffset = baseOffset + docStart,
                    EndOffset = baseOffset + docStart + remaining.Length
                });
            }
        }
    }

    private static List<string> SplitOnSeparator(string text, string separator)
    {
        var result = new List<string>();
        if (separator == "")
        {
            for (var i = 0; i < text.Length; i++)
            {
                result.Add(text[i].ToString());
            }
            return result;
        }

        var parts = text.Split(new[] { separator }, StringSplitOptions.None);
        for (var i = 0; i < parts.Length; i++)
        {
            result.Add(parts[i]);
            if (i < parts.Length - 1)
            {
                result.Add(separator);
            }
        }

        return result;
    }

    private static string GetOverlapText(string docText, int overlap)
    {
        if (docText.Length <= overlap) return docText;
        return docText[^overlap..];
    }
}
