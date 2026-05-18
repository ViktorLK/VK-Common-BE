using System;
using VK.Blocks.AI.Vectorics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Re-Ranking feature.
/// </summary>
[VKFeature(typeof(VectoricsFeature), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKReRankingOptions : IVKReRankingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Re-Ranking feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = false;

    // --- Connection ---
    public VKAIProviderType? Provider { get; init; }
    public string? ModelId { get; init; }
    public VKSensitiveString? ApiKey { get; init; }
    public string? Endpoint { get; init; }

    // --- Resilience ---
    public TimeSpan? Timeout { get; init; }
    public int? RetryCount { get; init; }
    public int? CircuitBreakerThreshold { get; init; }
    public TimeSpan? CircuitBreakerBreakDuration { get; init; }
}
