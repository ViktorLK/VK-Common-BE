using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.Retrieval;

/// <summary>
/// Options for configuring the AI Vector Store Retrieval feature.
/// </summary>
[VKFeature(typeof(VKAIVectorStoreBlock))]
public sealed partial record VKVectorStoreRetrievalOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Vector Store Retrieval feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
