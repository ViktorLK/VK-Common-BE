namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// Represents a chunk of a document.
/// </summary>
public sealed record VKDocumentChunk
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
