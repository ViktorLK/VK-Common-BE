using VK.Blocks.VectorStore;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Represents a segmented block of text (chunk) derived from an ingested document.
/// </summary>
public sealed record VKDocumentChunk
{
    /// <summary>
    /// Gets the unique identifier for the chunk.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the actual text content of the chunk.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the parent document identifier this chunk belongs to.
    /// </summary>
    public required string DocumentId { get; init; }

    /// <summary>
    /// Gets the metadata associated with the chunk.
    /// </summary>
    public required VKVectorMetadata Metadata { get; init; }
}
