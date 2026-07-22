using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Privacy Guard (PII detection) feature.
/// </summary>
public sealed partial record VKPrivacyOptions : IVKToggleableBlockOptions, IVKAIProviderOptions, IVKAIGovernanceOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Privacy Guard feature is enabled.
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

    // --- Privacy Specific ---
    [VKRequestOverride]
    public System.Collections.Generic.IReadOnlyList<string>? Categories { get; init; } = ["Email", "Phone", "Person", "Location"];
    [VKRequestOverride]
    public char? MaskingChar { get; init; } = '*';
}
