using VK.Blocks.AI.Tokenics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Token Costing feature.
/// </summary>
[VKFeature(typeof(TokenicsFeature))]
public sealed partial record VKCostingOptions : IVKToggleableBlockOptions
{

    /// <summary>
    /// Gets or sets a value indicating whether Token Costing is enabled.
    /// Defaults to false.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets or sets the default currency for cost calculation.
    /// Defaults to USD.
    /// </summary>
    public string DefaultCurrency { get; init; } = "USD";
}
