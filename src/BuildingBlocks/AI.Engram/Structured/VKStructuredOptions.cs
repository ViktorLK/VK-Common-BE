using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Configuration options for the Structured deterministic KV memory feature.
/// Follows BB.05 / AP.01.
/// </summary>
public sealed partial record VKStructuredOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether the Structured memory subsystem is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the maximum number of structured facts allowed per tenant/scope.
    /// Defaults to 1000.
    /// </summary>
    public int MaxFactsPerTenant { get; init; } = 1000;
}
