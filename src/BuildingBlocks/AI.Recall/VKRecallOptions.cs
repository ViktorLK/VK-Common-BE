using VK.Blocks.Core;

namespace VK.Blocks.AI.Recall;

/// <summary>
/// Root options for the AI Recall building block.
/// Acts as the identity anchor for the block.
/// </summary>
public sealed record VKRecallOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for AI Recall options.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKRecallBlock.BlockName}";

    /// <summary>
    /// Gets or sets a value indicating whether AI Recall features are enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
