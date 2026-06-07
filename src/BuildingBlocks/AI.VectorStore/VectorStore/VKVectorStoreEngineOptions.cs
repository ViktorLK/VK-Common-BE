using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.VectorStore;

/// <summary>
/// Options for configuring the AI Vector Store Engine feature.
/// </summary>
[VKFeature(typeof(VKAIVectorStoreBlock))]
public sealed partial record VKVectorStoreEngineOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Vector Store Engine feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
