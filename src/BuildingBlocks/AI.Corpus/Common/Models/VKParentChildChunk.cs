namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Model representing the mapping between a child search chunk and its parent document context.
/// </summary>
public sealed record VKParentChildChunk
{
    /// <summary>
    /// Gets the unique identifier of the parent document/chunk.
    /// </summary>
    public required string ParentId { get; init; }

    /// <summary>
    /// Gets the full raw content of the parent document/chunk.
    /// </summary>
    public required string ParentContent { get; init; }

    /// <summary>
    /// Gets the unique identifier of the child search chunk.
    /// </summary>
    public required string ChildId { get; init; }

    /// <summary>
    /// Gets the content of the child search chunk.
    /// </summary>
    public required string ChildContent { get; init; }

    /// <summary>
    /// Gets the start character offset of the child chunk within the parent content.
    /// </summary>
    public int StartOffset { get; init; }

    /// <summary>
    /// Gets the end character offset of the child chunk within the parent content.
    /// </summary>
    public int EndOffset { get; init; }
}
