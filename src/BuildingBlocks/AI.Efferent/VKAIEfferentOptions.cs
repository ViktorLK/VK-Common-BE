using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;

/// <summary>
/// Root options for the AI Efferent building block.
/// Acts as the identity anchor for the block.
/// </summary>
public sealed record VKAIEfferentOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for AI Efferent options.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAIEfferentBlock.BlockName}";

    /// <summary>
    /// Gets or sets a value indicating whether AI Efferent features are enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
