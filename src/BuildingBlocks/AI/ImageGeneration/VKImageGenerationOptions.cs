using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Options for the image generation feature.
/// </summary>
public sealed partial record VKImageGenerationOptions : IVKToggleableBlockOptions, IVKAIProviderOptions, IVKAIGovernanceOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the image generation feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    // --- Connection ---
    [VKRequestOverride]
    public VKAIProviderType? Provider { get; init; }
    [VKRequestOverride]
    public string? ModelId { get; init; }
    [VKRequestOverride]
    public VKSensitiveString? ApiKey { get; init; }
    [VKRequestOverride]
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

    // --- Specific Settings ---
    [VKRequestOverride]
    public int? Width { get; init; } = 1024;
    [VKRequestOverride]
    public int? Height { get; init; } = 1024;
    [VKRequestOverride]
    public string? AspectRatio { get; init; }
}
