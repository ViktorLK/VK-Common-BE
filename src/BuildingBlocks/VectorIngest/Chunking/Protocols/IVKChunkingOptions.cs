using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest; // [AP.03] public API surface in flat root namespace

/// <summary>
/// Aggregates all static Chunking configuration options.
/// </summary>
public interface IVKChunkingOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the maximum character size of each chunk.
    /// </summary>
    int ChunkSize { get; }

    /// <summary>
    /// Gets the overlap size between consecutive chunks.
    /// </summary>
    int ChunkOverlap { get; }
}
