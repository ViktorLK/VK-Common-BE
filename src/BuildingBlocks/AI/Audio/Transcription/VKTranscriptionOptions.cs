using System;
using VK.Blocks.AI.Audio.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Audio Transcription feature.
/// </summary>
[VKFeature(typeof(AudioFeature), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKTranscriptionOptions : IVKTranscriptionOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Transcription feature is enabled.
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

    // --- Audio Transcription Specific ---
    public string? Language { get; init; }
    public bool? Translate { get; init; } = false;
    public bool? EnableDiarization { get; init; } = false;
    public bool? EnableTimestamps { get; init; } = true;
    public float? Temperature { get; init; } = 0.0f;
    public string? ResponseFormat { get; init; } = "json";
}
