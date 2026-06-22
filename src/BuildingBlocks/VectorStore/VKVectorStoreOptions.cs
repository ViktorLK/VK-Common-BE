using VK.Blocks.Core;

namespace VK.Blocks.VectorStore;

/// <summary>
/// Options for configuring the AI Vector Store.
/// </summary>
public sealed record VKVectorStoreOptions : IVKToggleableBlockOptions
{
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKVectorStoreBlock.BlockName}";

    /// <summary>
    /// Gets or sets a value indicating whether the Vector Store block is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
