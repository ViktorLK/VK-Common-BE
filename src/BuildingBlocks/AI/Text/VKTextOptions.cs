using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Options for the text generation feature.
/// </summary>
[VKFeature(typeof(VKAIBlock), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKTextOptions : IVKTextSettings, IVKToggleableBlockOptions
{
    /// <inheritdoc />
    public bool Enabled { get; init; } = true;

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

    // --- Text Specific ---
    public float? Temperature { get; init; } = 0.7f;
    public int? MaxTokens { get; init; } = 512;
    public float? TopP { get; init; } = 1.0f;
    public float? FrequencyPenalty { get; init; } = 0.0f;
    public float? PresencePenalty { get; init; } = 0.0f;
    public System.Collections.Generic.IReadOnlyList<string>? StopSequences { get; init; } = [];
}
