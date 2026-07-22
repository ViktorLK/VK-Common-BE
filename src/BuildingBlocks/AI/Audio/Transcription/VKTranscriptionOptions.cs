using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Audio Transcription feature.
/// </summary>
public sealed partial record VKTranscriptionOptions : IVKToggleableBlockOptions, IVKAIProviderOptions, IVKAIGovernanceOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Transcription feature is enabled.
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

    // --- Audio Transcription Specific ---
    [VKRequestOverride]
    public string? Language { get; init; }
    [VKRequestOverride]
    public bool? Translate { get; init; } = false;
    [VKRequestOverride]
    public bool? EnableDiarization { get; init; } = false;
    [VKRequestOverride]
    public bool? EnableTimestamps { get; init; } = true;
    [VKRequestOverride]
    public float? Temperature { get; init; } = 0.0f;
    [VKRequestOverride]
    public string? ResponseFormat { get; init; } = "json";
}
