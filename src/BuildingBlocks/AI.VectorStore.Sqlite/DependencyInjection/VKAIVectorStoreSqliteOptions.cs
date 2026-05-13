using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.Sqlite;

/// <summary>
/// Options for configuring the SQLite AI Vector Store extension.
/// </summary>
public sealed record VKAIVectorStoreSqliteOptions : IVKBlockOptions
{
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAIVectorStoreSqliteBlock.BlockName}";

    /// <summary>
    /// Gets or sets a value indicating whether the SQLite Vector Store extension is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the SQLite connection string.
    /// </summary>
    public string Connection { get; init; } = string.Empty;

    /// <summary>
    /// Gets the embedding dimension for the vector store.
    /// Required for sqlite-vec virtual table initialization.
    /// </summary>
    public int EmbeddingDimension { get; init; } = 1536;
}
