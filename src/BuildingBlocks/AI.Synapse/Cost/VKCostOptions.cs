using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Configuration options for AI Synapse Cost & Token Pricing feature slice.
/// Follows AP.04 and BB.05.
/// </summary>
public sealed partial record VKCostOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets whether cost calculation is enabled. Default is true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets custom pricing models to override or extend baseline pricing.
    /// </summary>
    public IReadOnlyList<VKModelPricing> CustomPricing { get; init; } = [];
}
