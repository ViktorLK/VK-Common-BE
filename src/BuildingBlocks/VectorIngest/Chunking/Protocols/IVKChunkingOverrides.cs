namespace VK.Blocks.VectorIngest; // [AP.03] public API surface in flat root namespace

/// <summary>
/// Defines request-level overrides for chunking parameters.
/// </summary>
public interface IVKChunkingOverrides
{
    /// <summary>
    /// Gets the overridden maximum character size of each chunk.
    /// </summary>
    int? ChunkSize { get; init; }

    /// <summary>
    /// Gets the overridden overlap size between consecutive chunks.
    /// </summary>
    int? ChunkOverlap { get; init; }
}
