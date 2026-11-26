using System;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Source contract for resolving model pricing rates.
/// </summary>
public interface IVKAIPriceSource
{
    /// <summary>
    /// Gets the effective pricing for the specified provider and model at the given point in time.
    /// </summary>
    VKModelPricing? GetPricing(string provider, string modelId, DateTimeOffset? asOf = null);
}
