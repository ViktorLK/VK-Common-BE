namespace VK.Blocks.VectorIngest; // [AP.03] public API surface flat root namespace

/// <summary>
/// Specifies the type of text chunking strategy to use.
/// </summary>
public enum VKChunkerType
{
    /// <summary>
    /// Fixed size text chunker with sliding window overlap.
    /// </summary>
    FixedSize,

    /// <summary>
    /// Recursive text chunker trying to maintain semantic boundaries (paragraphs, sentences, spaces).
    /// </summary>
    Recursive,

    /// <summary>
    /// Chunks text dynamically based on semantic similarity of sentences using embeddings.
    /// </summary>
    Semantic,

    /// <summary>
    /// Chunks text into larger parent segments and smaller child segments, linking child segments back to their parents.
    /// </summary>
    Hierarchical
}
