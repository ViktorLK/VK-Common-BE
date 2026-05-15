using VK.Blocks.AI.VectorStore;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// Configuration settings for the Retrieval feature.
/// </summary>
public sealed record VKRetrievalOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for Retrieval options.
    /// </summary>
    public static string SectionName => VKAIVectorStoreOptions.SectionName + ":Retrieval";

    /// <summary>
    /// Gets or sets a value indicating whether RAG feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the default collection name for the vector store.
    /// </summary>
    public string DefaultCollection { get; init; } = "default";
    public int ChunkSize { get; init; } = 512;
    public int ChunkOverlap { get; init; } = 64;
    /// <summary>
    /// Gets or sets the maximum number of results to retrieve.
    /// </summary>
    public int MaxResultsToRetrieve { get; init; } = 5;

    /// <summary>
    /// Gets or sets the minimum relevance score (0.0 to 1.0).
    /// </summary>
    public float MinRelevanceScore { get; init; } = 0.7f;

    /// <summary>
    /// Gets the vector store options.
    /// </summary>
    public VKAIVectorStoreOptions VectorStore { get; init; } = new();
}
