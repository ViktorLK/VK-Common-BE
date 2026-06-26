namespace VK.Blocks.VectorSearch;

/// <summary>
/// Represents a document targeted for search and recall.
/// </summary>
public sealed record VKDocument
{
    /// <summary>
    /// Gets the unique identifier of the document.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the content of the document.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the metadata of the document.
    /// </summary>
    public required string Metadata { get; init; }
}
