using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Root options for the AI Psyche building block.
/// Acts as the identity anchor for the block.
/// </summary>
public sealed record VKAIPsycheOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for AI Psyche options.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAIPsycheBlock.BlockName}";

    /// <summary>
    /// Gets or sets a value indicating whether AI Psyche features are enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
