using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Root configuration settings for the Vectorics pillar (Embeddings, Retrieval).
/// Following AP.05: Hierarchical configuration pattern.
/// </summary>
[VKFeature(typeof(VKAIBlock))]
public sealed partial record VKVectoricsOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether all Vectorics features are enabled.
    /// Defaults to false.
    /// </summary>
    public bool Enabled { get; init; } = false;
}
