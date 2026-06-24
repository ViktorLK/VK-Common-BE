using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Options for the Query Rewrite stage.
/// </summary>
[VKFeature(typeof(VKVectorSearchBlock), GenerateArgs = true)]
public sealed partial record VKQueryRewriteOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Query Rewrite stage is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
