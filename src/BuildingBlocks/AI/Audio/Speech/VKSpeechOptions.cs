using System;
using VK.Blocks.AI.Audio.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Audio Speech (TTS) feature.
/// </summary>
[VKFeature(typeof(AudioFeature), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKSpeechOptions : IVKSpeechSettings, IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Speech feature is enabled.
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

    // --- Audio Speech Specific ---
    public string? Voice { get; init; } = "alloy";
    public string? AudioFormat { get; init; } = "mp3";
    public float? Speed { get; init; } = 1.0f;
    public float? Pitch { get; init; } = 0.0f;
    public string? Style { get; init; }
}
