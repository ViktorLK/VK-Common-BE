using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Root options for the AI.Corpus building block.
/// Acts as the identity anchor for the block.
/// Follows BB.05 / BB.07.
/// </summary>
public sealed record VKCorpusOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// The configuration section name for AI.Corpus options.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:AI:Corpus";

    /// <summary>
    /// Gets a value indicating whether the AI.Corpus block is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
