using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Root options for the AI Afferent building block.
/// Acts as the identity anchor for the block.
/// </summary>
public sealed record VKAIAfferentOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for AI Afferent options.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAIAfferentBlock.BlockName}";

    /// <summary>
    /// Gets or sets a value indicating whether AI Afferent features are enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
