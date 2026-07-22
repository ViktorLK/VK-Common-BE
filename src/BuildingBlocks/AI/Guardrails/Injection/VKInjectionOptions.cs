using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Injection Guard (Prompt injection defense) feature.
/// </summary>
public sealed partial record VKInjectionOptions : IVKToggleableBlockOptions, IVKAIProviderOptions, IVKAIGovernanceOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Injection Guard feature is enabled.
    /// Defaults to false.
    /// </summary>
    public bool Enabled { get; init; } = false;

    // --- Connection ---
    public VKAIProviderType? Provider { get; init; }
    public string? ModelId { get; init; }
    public VKSensitiveString? ApiKey { get; init; }
    public string? Endpoint { get; init; }

    // --- Resilience ---
    [VKRequestOverride]
    public TimeSpan? Timeout { get; init; }
    [VKRequestOverride]
    public int? RetryCount { get; init; }
    public int? CircuitBreakerThreshold { get; init; }
    public TimeSpan? CircuitBreakerBreakDuration { get; init; }

    // --- Audit ---
    [VKRequestOverride]
    public bool? EnableAudit { get; init; }

    // --- Quota ---
    public long? GlobalTokenLimit { get; init; }
    public long? MonthlyTokenBudget { get; init; }
    public int? RateLimitPerMinute { get; init; }

    // --- Safety ---
    [VKRequestOverride]
    public bool? EnableContentFilter { get; init; }

    // --- Injection Specific ---
    [VKRequestOverride]
    public float? BlockThreshold { get; init; } = 0.8f;
}
