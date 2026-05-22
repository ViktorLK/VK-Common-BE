using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// Options for configuring the AI Vector Store.
/// </summary>
public sealed record VKAIVectorStoreOptions : IVKBlockOptions
{
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAIVectorStoreBlock.BlockName}";

    /// <summary>
    /// Gets or sets a value indicating whether the Vector Store block is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the type of the vector store.
    /// </summary>
    public VKAIVectorStoreType Type { get; init; } = VKAIVectorStoreType.InMemory;

    /// <summary>
    /// Gets the default maximum number of results to return.
    /// </summary>
    public int DefaultLimit { get; init; } = 5;

    /// <summary>
    /// Gets the default minimum similarity score (0.0 to 1.0).
    /// </summary>
    public float DefaultMinScore { get; init; } = 0.7f;
}
