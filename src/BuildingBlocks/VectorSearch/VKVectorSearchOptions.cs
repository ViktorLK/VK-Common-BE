using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Root options for the AI Recall building block.
/// Acts as the identity anchor for the block.
/// </summary>
public sealed record VKVectorSearchOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for AI Recall options.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKVectorSearchBlock.BlockName}";

    /// <summary>
    /// Gets or sets a value indicating whether AI Recall features are enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
