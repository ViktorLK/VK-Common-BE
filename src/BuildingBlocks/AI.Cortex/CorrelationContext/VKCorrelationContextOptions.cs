using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Options for Correlation Context slice.
/// Follows BB.05.
/// </summary>
public sealed partial record VKCorrelationContextOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether Correlation Context tracking is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
