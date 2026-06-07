using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines generic parameters for text chunking operations.
/// </summary>
public interface IVKChunkingOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets the chunk size.
    /// </summary>
    int ChunkSize { get; init; }

    /// <summary>
    /// Gets the overlap between chunks.
    /// </summary>
    int ChunkOverlap { get; init; }
}
