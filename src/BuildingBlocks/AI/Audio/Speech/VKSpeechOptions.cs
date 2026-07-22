using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Audio Speech (TTS) feature.
/// </summary>
public sealed partial record VKSpeechOptions : IVKToggleableBlockOptions, IVKAIProviderOptions, IVKAIGovernanceOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Speech feature is enabled.
    /// Defaults to false.
    /// </summary>
    public bool Enabled { get; init; } = false;

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

    // --- Audio Speech Specific ---
    [VKRequestOverride]
    public string? Voice { get; init; } = "alloy";
    [VKRequestOverride]
    public string? AudioFormat { get; init; } = "mp3";
    [VKRequestOverride]
    public float? Speed { get; init; } = 1.0f;
    [VKRequestOverride]
    public float? Pitch { get; init; } = 0.0f;
    [VKRequestOverride]
    public string? Style { get; init; }
}
