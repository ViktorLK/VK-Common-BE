using System;
using VK.Blocks.AI.Audio.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Audio Transcription feature.
/// </summary>
public sealed record VKAudioTranscriptionOptions : IVKAIProviderSettings, IVKAIGovernanceSettings, IVKToggleableBlockOptions
{
    /// <summary>
    /// The configuration section name for Audio Transcription options.
    /// </summary>
    public static string SectionName => $"{VKAIOptions.SectionName}:{AudioConstants.TranscriptionFeatureName}";

    /// <summary>
    /// Gets or sets a value indicating whether Transcription feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    // --- Connection ---

    /// <inheritdoc />
    public VKAIProviderType? Provider { get; init; }

    /// <inheritdoc />
    public string? ModelId { get; init; }

    /// <inheritdoc />
    public VKSensitiveString? ApiKey { get; init; }

    /// <inheritdoc />
    public string? Endpoint { get; init; }

    // --- Resilience ---

    /// <inheritdoc />
    public TimeSpan? Timeout { get; init; }

    /// <inheritdoc />
    public int? RetryCount { get; init; }

    /// <inheritdoc />
    public int? CircuitBreakerThreshold { get; init; }

    /// <inheritdoc />
    public TimeSpan? CircuitBreakerBreakDuration { get; init; }

    // --- Audit ---

    /// <inheritdoc />
    public bool? EnableAudit { get; init; }

    // --- Quota ---

    /// <inheritdoc />
    public long? GlobalTokenLimit { get; init; }

    /// <inheritdoc />
    public long? MonthlyTokenBudget { get; init; }

    /// <inheritdoc />
    public int? RateLimitPerMinute { get; init; }

    // --- Safety ---

    /// <inheritdoc />
    public bool? EnableContentFilter { get; init; }

    // --- Audio Transcription Specific ---

    /// <summary>
    /// Gets or sets the language for transcription (e.g., "en-US", "zh-CN").
    /// </summary>
    public string? Language { get; init; }
}
