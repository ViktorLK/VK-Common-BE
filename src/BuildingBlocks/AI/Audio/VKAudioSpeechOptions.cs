using System;
using VK.Blocks.AI.Audio.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Audio Speech (TTS) feature.
/// </summary>
public sealed record VKAudioSpeechOptions : IVKAIProviderSettings, IVKAIGovernanceSettings, IVKToggleableBlockOptions
{
    /// <summary>
    /// The configuration section name for Audio Speech options.
    /// </summary>
    public static string SectionName => $"{VKAIOptions.SectionName}:{AudioConstants.SpeechFeatureName}";

    /// <summary>
    /// Gets or sets a value indicating whether Speech feature is enabled.
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

    // --- Audio Speech Specific ---

    /// <summary>
    /// Gets or sets the voice for speech generation.
    /// </summary>
    public string Voice { get; init; } = "alloy";
}
