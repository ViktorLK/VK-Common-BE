using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Root options for the AI Cognitive building block.
/// Acts as the identity anchor for the block.
/// </summary>
public sealed record VKAICognitiveOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for AI Cognitive options.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAICognitiveBlock.BlockName}";

    /// <summary>
    /// Gets or sets a value indicating whether AI Cognitive features are enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
