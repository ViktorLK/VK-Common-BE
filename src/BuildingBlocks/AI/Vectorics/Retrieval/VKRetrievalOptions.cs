using System;
using VK.Blocks.AI.Vectorics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Retrieval feature.
/// </summary>
[VKFeature(typeof(VectoricsFeature), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKRetrievalOptions : IVKRetrievalOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Retrieval feature is enabled.
    /// Defaults to false.
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

    // --- Audit ---
    public bool? EnableAudit { get; init; }

    // --- Quota ---
    public long? GlobalTokenLimit { get; init; }
    public long? MonthlyTokenBudget { get; init; }
    public int? RateLimitPerMinute { get; init; }

    // --- Safety ---
    public bool? EnableContentFilter { get; init; }

    // --- Retrieval Specific ---
    public int? TopK { get; init; } = 5;
    public double? MinScore { get; init; }
    public bool? EnableTemporalWeighting { get; init; }
    public double? DecayRate { get; init; }
}
