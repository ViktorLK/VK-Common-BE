using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Options for configuring the Vector Search execution pipeline.
/// </summary>
[VKFeature(typeof(VKVectorSearchBlock))]
public sealed partial record VKPipelineOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the search pipeline is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
