using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Configuration settings for the Pattern feature.
/// </summary>

public sealed partial record VKPatternOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Pattern feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
