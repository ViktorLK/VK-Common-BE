using VK.Blocks.Core;

namespace VK.Blocks.VectorStore;

/// <summary>
/// Options for configuring the AI Vector Store VecEngine feature.
/// </summary>
[VKFeature(typeof(VKVectorStoreBlock))]
public sealed partial record VKVecEngineOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Vector Store VecEngine feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
