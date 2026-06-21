using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest;

/// <summary>
/// Options for configuring the AI Ingest block.
/// </summary>
public sealed record VKAIIngestOptions : IVKToggleableBlockOptions
{
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:AIIngest";

    /// <summary>
    /// Gets or sets a value indicating whether the Ingest block is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
