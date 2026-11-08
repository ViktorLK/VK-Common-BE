using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Options for the Session thread feature in Psyche.
/// Follows BB.05 (Options pattern with sealed record).
/// </summary>
[VKOptions(ArgsMode = VKArgsGenerationMode.Explicit, GenerateMerge = true)]
public sealed partial record VKSessionOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Session feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
