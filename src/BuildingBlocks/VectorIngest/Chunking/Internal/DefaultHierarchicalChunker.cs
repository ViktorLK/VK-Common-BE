using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.VectorIngest;
using VK.Blocks.VectorIngest.Chunking.Internal;

namespace VK.Blocks.VectorIngest.Chunking.Internal; // [AP.03] Internal namespace

/// <summary>
/// Default hierarchical chunker partitioning text into parent-child segments.
/// </summary>
internal sealed class DefaultHierarchicalChunker(
    DefaultRecursiveChunker recursiveChunker,
    IVKChunkingOptionsProvider optionsProvider) : IVKTextChunker // [AP.01] sealed default, [AP.03] Naming taxonomy
{
    private readonly DefaultRecursiveChunker _recursiveChunker = VKGuard.NotNull(recursiveChunker);
    private readonly IVKChunkingOptionsProvider _optionsProvider = VKGuard.NotNull(optionsProvider);

    /// <inheritdoc />
    public async Task<VKResult<IReadOnlyList<VKChunk>>> ChunkAsync(
        string text,
        VKChunkingArgs args,
        CancellationToken cancellationToken = default) // [CS.03] Async + CancellationToken
    {
        VKGuard.NotNull(text); // [AP.01] VKGuard boundary
        VKGuard.NotNull(args);

        if (string.IsNullOrEmpty(text))
        {
            IReadOnlyList<VKChunk> emptyList = [];
            return VKResult.Success(emptyList);
        }

        var options = _optionsProvider.GetOptions(args);

        // 1. Generate Parent Chunks (using a larger multiplier, e.g., ChunkSize * 4)
        var parentArgs = new VKChunkingArgs
        {
            ChunkSize = args.ChunkSize is not null ? args.ChunkSize * 4 : options.ChunkSize * 4,
            ChunkOverlap = args.ChunkOverlap is not null ? args.ChunkOverlap * 4 : options.ChunkOverlap * 4
        };

        var parentResult = await _recursiveChunker.ChunkAsync(text, parentArgs, cancellationToken).ConfigureAwait(false);
        if (parentResult.IsFailure)
        {
            return VKResult.Failure<IReadOnlyList<VKChunk>>(parentResult.Errors); // [CS.01] Result flow
        }

        var parentChunks = parentResult.Value;
        var allChunks = new List<VKChunk>();
        var childIndex = 0;

        // 2. For each Parent Chunk, generate child chunks and link them
        foreach (var parent in parentChunks)
        {
            var childResult = await _recursiveChunker.ChunkAsync(parent.Content, args, cancellationToken).ConfigureAwait(false);
            if (childResult.IsFailure)
            {
                return VKResult.Failure<IReadOnlyList<VKChunk>>(childResult.Errors);
            }

            foreach (var child in childResult.Value)
            {
                // Align offsets relative to original source text
                var alignedStart = parent.StartOffset + child.StartOffset;
                var alignedEnd = parent.StartOffset + child.EndOffset;

                var childChunk = child with
                {
                    ChunkIndex = childIndex++,
                    StartOffset = alignedStart,
                    EndOffset = alignedEnd
                };

                // Link child to parent via Metadata
                childChunk.Metadata["parentId"] = parent.Id;
                childChunk.Metadata["parentContent"] = parent.Content;
                childChunk.Metadata["chunkType"] = "child";

                allChunks.Add(childChunk);
            }
        }

        IReadOnlyList<VKChunk> result = allChunks;
        return VKResult.Success(result);
    }
}
