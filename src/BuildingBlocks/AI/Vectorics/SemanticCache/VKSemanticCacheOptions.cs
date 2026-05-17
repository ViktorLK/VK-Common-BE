using System;
using VK.Blocks.AI.Vectorics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Semantic Caching feature.
/// Following industrial standards for Vectorics features.
/// </summary>
[VKFeature(typeof(VectoricsFeature), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKSemanticCacheOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Semantic Caching is enabled.
    /// Defaults to false.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets or sets the similarity threshold for a cache hit.
    /// Range [0, 1]. Defaults to 0.95.
    /// </summary>
    public float MinSimilarity { get; init; } = 0.95f;

    /// <summary>
    /// Gets or sets the TTL (Time To Live) for cached items.
    /// Defaults to 24 hours.
    /// </summary>
    public TimeSpan Ttl { get; init; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Gets or sets the execution timeout for cache operations.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable metrics for cache hits/misses.
    /// </summary>
    public bool EnableMetrics { get; init; } = true;
}
