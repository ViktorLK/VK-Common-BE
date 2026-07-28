using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Configuration settings for the Memory feature (Persistence & Search).
/// </summary>
public sealed partial record VKMemoryOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Memory feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the default relevance threshold for memory search.
    /// </summary>
    [VKRequestOverride]
    public float? DefaultMinScore { get; init; } = 0.7f;

    /// <summary>
    /// Gets or sets the default maximum number of memories to return in search when TopK is unspecified.
    /// </summary>
    [VKRequestOverride]
    public int? DefaultTopK { get; init; } = 5;

    /// <summary>
    /// Gets or sets the maximum number of long-term memory entries to inject into Psyche prompt context.
    /// Defaults to 5.
    /// </summary>
    [VKRequestOverride]
    public int? MaxMemoryEntriesToInject { get; init; } = 5;
}
