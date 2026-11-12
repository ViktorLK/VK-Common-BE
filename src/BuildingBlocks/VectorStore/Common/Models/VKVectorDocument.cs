namespace VK.Blocks.VectorStore;

/// <summary>
/// Universal text document record stored alongside its embedding vector in a vector collection.
/// </summary>
public sealed record VKVectorDocument
{
    /// <summary>
    /// Gets the text content of the document or chunk.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the metadata associated with the document.
    /// </summary>
    public required VKVectorMetadata Metadata { get; init; }
}

