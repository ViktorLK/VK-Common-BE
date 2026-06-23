using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Options for the Vector Reranking stage.
/// </summary>
[VKFeature(typeof(VKVectorSearchBlock), GenerateArgs = true)]
public sealed partial record VKVectorRerankingOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Reranking stage is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
