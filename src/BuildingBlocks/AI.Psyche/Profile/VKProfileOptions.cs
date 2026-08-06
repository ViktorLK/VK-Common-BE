using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Configuration options for the Profile feature slice in Psyche.
/// Follows BB.05 and BB.07.
/// </summary>
public sealed partial record VKProfileOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Profile feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
