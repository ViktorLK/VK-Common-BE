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
    /// Gets the connection string or endpoint for the vector store.
    /// </summary>
    public string? Connection { get; init; }

    /// <summary>
    /// Gets the API key if required.
    /// </summary>
    public string? ApiKey { get; init; }
}
