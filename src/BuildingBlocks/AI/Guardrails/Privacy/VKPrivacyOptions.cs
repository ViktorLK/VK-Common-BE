using System;
using VK.Blocks.AI.Guardrails.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Privacy Guard (PII detection) feature.
/// </summary>
[VKFeature(typeof(GuardrailsFeature), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKPrivacyOptions : IVKPrivacySettings, IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Privacy Guard feature is enabled.
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

    // --- Privacy Specific ---
    public System.Collections.Generic.IReadOnlyList<string>? Categories { get; init; } = ["Email", "Phone", "Person", "Location"];
    public char? MaskingChar { get; init; } = '*';
}
