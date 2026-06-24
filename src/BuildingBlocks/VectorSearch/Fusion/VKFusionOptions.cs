using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Options for configuring the Fusion stage.
/// </summary>
[VKFeature(typeof(VKVectorSearchBlock))]
public sealed partial record VKFusionOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Fusion stage is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
