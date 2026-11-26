using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Configuration options for the AI routing and fallback feature.
/// Follows AP.04 (IVKBlockOptions) and BB.05 (immutable record with init).
/// </summary>
[VKOptions]
public sealed partial record VKRoutingOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets whether AI routing is enabled. When <see langword="false"/>, routing profile and connection validation is bypassed for cost-only gateway mode.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the routing strategy used to prioritize candidate providers.
    /// </summary>
    public VKAIRoutingStrategy Strategy { get; init; } = VKAIRoutingStrategy.Priority;

    /// <summary>
    /// Gets the configured named routing profiles.
    /// </summary>
    public IReadOnlyList<VKAIProfile> Profiles { get; init; } = Array.Empty<VKAIProfile>();

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
}
