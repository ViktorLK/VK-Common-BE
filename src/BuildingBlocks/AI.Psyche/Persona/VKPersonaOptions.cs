using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Configuration settings for the Persona feature.
/// </summary>

public sealed partial record VKPersonaOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Persona feature is enabled.
    /// Defaults to true.
    /// </summary>
    [VKRequestOverride]
    public bool Enabled { get; init; } = true;
}
