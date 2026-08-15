using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Configuration options for the fallback strategy slice.
/// </summary>
public sealed partial record VKFallbackOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets whether fallback execution is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
