using System;
using VK.Blocks.AI.Moderation.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Moderation feature.
/// </summary>
public sealed record VKModerationOptions : IVKAIProviderSettings, IVKAIGovernanceSettings, IVKToggleableBlockOptions
{
    /// <summary>
    /// The configuration section name for Moderation options.
    /// </summary>
    public static string SectionName => $"{VKAIOptions.SectionName}:{ModerationConstants.FeatureName}";

    /// <summary>
    /// Gets or sets a value indicating whether Moderation feature is enabled.
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

    // --- Moderation Specific ---

    /// <summary>
    /// Gets or sets a value indicating whether to automatically block requests that fail moderation.
    /// Defaults to true.
    /// </summary>
    public bool AutoBlockOnFailure { get; init; } = true;

    /// <summary>
    /// Gets or sets the sensitivity threshold for moderation.
    /// </summary>
    public float SensitivityThreshold { get; init; } = 0;
}
