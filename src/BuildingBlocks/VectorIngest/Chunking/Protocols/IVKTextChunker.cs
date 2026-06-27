using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest; // [AP.03] public API surface in flat root namespace

/// <summary>
/// Defines the public contract for slicing text into smaller semantic or size-bounded chunks.
/// </summary>
public interface IVKTextChunker
{
    /// <summary>
    /// Slices the input text into a list of chunks based on the provided options asynchronously.
    /// </summary>
    /// <param name="text">The source text to chunk.</param>
    /// <param name="args">The chunking configuration options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resulting list of chunks.</returns>
    Task<VKResult<IReadOnlyList<VKChunk>>> ChunkAsync(
        string text,
        VKChunkingArgs args,
        CancellationToken cancellationToken = default); // [CS.01] Result Pattern, [CS.03] Async Everywhere
}
