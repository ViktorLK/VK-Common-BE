using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Configuration settings for the Pattern feature.
/// </summary>

[VKOptions(ArgsMode = VKArgsGenerationMode.Explicit, GenerateMerge = true)]
public sealed partial record VKPatternOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Pattern feature is enabled.
    /// Defaults to true.
    /// </summary>
    [VKRequestOverride]
    public bool Enabled { get; init; } = true;
}
