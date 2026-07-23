using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Options for the text generation feature.
/// </summary>
public sealed partial record VKTextOptions : IVKToggleableBlockOptions, IVKAIProviderOptions, IVKAIGovernanceOptions
{
    /// <inheritdoc />
    [VKNoRequestOverride]
    public bool Enabled { get; init; } = true;

    // --- Connection ---
    public VKAIProviderType? Provider { get; init; }
    public string? ModelId { get; init; }
    public VKSensitiveString? ApiKey { get; init; }
    public string? Endpoint { get; init; }

    // --- Resilience ---
    public TimeSpan? Timeout { get; init; }
    public int? RetryCount { get; init; }

    [VKNoRequestOverride]
    public int? CircuitBreakerThreshold { get; init; }

    [VKNoRequestOverride]
    public TimeSpan? CircuitBreakerBreakDuration { get; init; }

    // --- Audit ---
    public bool? EnableAudit { get; init; }

    // --- Quota ---
    [VKNoRequestOverride]
    public long? GlobalTokenLimit { get; init; }

    [VKNoRequestOverride]
    public long? MonthlyTokenBudget { get; init; }

    [VKNoRequestOverride]
    public int? RateLimitPerMinute { get; init; }

    // --- Safety ---
    public bool? EnableContentFilter { get; init; }

    // --- Text Specific ---
    public float? Temperature { get; init; } = 0.7f;
    public int? MaxTokens { get; init; } = 512;
    public float? TopP { get; init; } = 1.0f;
    public float? FrequencyPenalty { get; init; } = 0.0f;
    public float? PresencePenalty { get; init; } = 0.0f;
    public System.Collections.Generic.IReadOnlyList<string>? StopSequences { get; init; } = [];
}
