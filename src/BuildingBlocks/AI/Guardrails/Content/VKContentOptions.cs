using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Content Guard (formerly Moderation) feature.
/// </summary>
public sealed partial record VKContentOptions : IVKToggleableBlockOptions, IVKAIProviderOptions, IVKAIGovernanceOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Content Guard feature is enabled.
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

    // --- Content Guard Specific ---
    [VKRequestOverride]
    public bool? AutoBlockOnFailure { get; init; } = true;
    [VKRequestOverride]
    public float? SensitivityThreshold { get; init; } = 0;
}
