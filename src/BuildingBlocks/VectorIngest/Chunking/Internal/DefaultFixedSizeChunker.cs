using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.VectorIngest;
using VK.Blocks.VectorIngest.Chunking.Internal;

namespace VK.Blocks.VectorIngest.Chunking.Internal; // [AP.03] Internal namespace

/// <summary>
/// Default fixed size text chunker with sliding window overlap.
/// </summary>
internal sealed class DefaultFixedSizeChunker(
    IVKGuidGenerator guidGenerator,
    IVKChunkingOptionsProvider optionsProvider) : IVKTextChunker // [AP.01] sealed default, [AP.03] internal scoping
{
    private readonly IVKGuidGenerator _guidGenerator = VKGuard.NotNull(guidGenerator);
    private readonly IVKChunkingOptionsProvider _optionsProvider = VKGuard.NotNull(optionsProvider);

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

        if (text.Length == 0)
        {
            IReadOnlyList<VKChunk> emptyList = [];
            return Task.FromResult(VKResult.Success(emptyList));
        }

        var chunks = new List<VKChunk>();
        var index = 0;
        var offset = 0;
        var textLength = text.Length;

        while (offset < textLength)
        {
            var length = Math.Min(options.ChunkSize, textLength - offset);
            var content = text.Substring(offset, length);

            chunks.Add(new VKChunk
            {
                Id = _guidGenerator.Create().ToString(), // [CS.06] Use IVKGuidGenerator
                Content = content,
                ChunkIndex = index++,
                StartOffset = offset,
                EndOffset = offset + length
            });

            offset += options.ChunkSize - options.ChunkOverlap;
            if (offset >= textLength || options.ChunkSize <= options.ChunkOverlap)
            {
                break;
            }
        }

        IReadOnlyList<VKChunk> result = chunks;
        return Task.FromResult(VKResult.Success(result));
    }
}
