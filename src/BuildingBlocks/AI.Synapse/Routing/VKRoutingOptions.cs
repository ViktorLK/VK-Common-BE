using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Configuration options for the AI routing and fallback feature.
/// Follows AP.04 (IVKBlockOptions) and BB.05 (immutable record with init).
/// </summary>
public sealed partial record VKRoutingOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the routing strategy used to prioritize candidate providers.
    /// </summary>
    public VKAIRoutingStrategy Strategy { get; init; } = VKAIRoutingStrategy.Preference;

    /// <summary>
    /// Gets the maximum number of fallback candidates to attempt before returning an error.
    /// </summary>
    public int MaxFallbackAttempts { get; init; } = 3;

    /// <summary>
    /// Gets whether to prefer lowest latency provider when multiple healthy candidates exist.
    /// </summary>
    public bool PreferLowestLatency { get; init; } = true;

    /// <summary>
    /// Gets whether cross-provider fallback (e.g. from OpenAI to Azure/Anthropic) is permitted.
    /// </summary>
    public bool EnableCrossProviderFallback { get; init; } = true;

    /// <summary>
    /// Gets the per-attempt request timeout duration. Default is 60 seconds.
    /// </summary>
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Gets the overall execution timeout duration across all fallback attempts. Default is 120 seconds.
    /// </summary>
    public TimeSpan OverallTimeout { get; init; } = TimeSpan.FromSeconds(120);
}
