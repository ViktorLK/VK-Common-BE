using VK.Blocks.Core;

namespace VK.Blocks.VectorStore.Sqlite;

/// <summary>
/// Default options for configuring the SQLite AI Vector Store extension.
/// </summary>
public sealed record VKVectorStoreSqliteDefaultsOptions : IVKBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => "AI:VectorStore:Sqlite";
    /// <summary>
    /// Gets the SQLite connection string.
    /// </summary>
    public string Connection { get; init; } = string.Empty;

    /// <summary>
    /// Gets the embedding dimension for the vector store.
    /// Required for sqlite-vec virtual table initialization.
    /// </summary>
    public int EmbeddingDimension { get; init; } = 1536;

    /// <summary>
    /// Gets the default search limit if not specified in search args.
    /// Following AP.05 hierarchical pattern.
    /// </summary>
    public int DefaultSearchLimit { get; init; } = 5;

    /// <summary>
    /// Gets the default minimum similarity score if not specified in search args.
    /// Following AP.05 hierarchical pattern.
    /// </summary>
    public float DefaultMinScore { get; init; } = 0.7f;
}
