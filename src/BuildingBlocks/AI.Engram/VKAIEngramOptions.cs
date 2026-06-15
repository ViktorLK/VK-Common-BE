using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Root options for the AI Engram building block.
/// Acts as the identity anchor for the block.
/// </summary>
public sealed record VKAIEngramOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for AI Engram options.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAIEngramBlock.BlockName}";

    /// <summary>
    /// Gets or sets a value indicating whether AI Engram features are enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
