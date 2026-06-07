using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// Options for configuring the AI Vector Store.
/// </summary>
public sealed record VKAIVectorStoreOptions : IVKToggleableBlockOptions
{
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAIVectorStoreBlock.BlockName}";

    /// <summary>
    /// Gets or sets a value indicating whether the Vector Store block is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
