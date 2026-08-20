using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Options for the Directive feature.
/// Follows BB.05 (Options pattern with sealed record).
/// </summary>
public sealed partial record VKDirectiveOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Directive feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
