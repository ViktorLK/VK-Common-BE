using VK.Blocks.AI.VectorStore;

namespace VK.Blocks.AI.Ingest;

/// <summary>
/// Represents a chunk of a document for vector storage.
/// </summary>
public sealed record VKVecDocumentChunk
{
    /// <summary>
    /// Gets the unique identifier for the chunk.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the content of the chunk.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the source document identifier.
    /// </summary>
    public required string DocumentId { get; init; }

    /// <summary>
    /// Gets the strongly-typed metadata for the chunk.
    /// </summary>
    public required VKAIVectorMetadata Metadata { get; init; }
}
