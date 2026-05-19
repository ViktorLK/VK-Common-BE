using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Options for the Cognitive Agents feature.
/// </summary>
[VKFeature(typeof(VKAICognitiveBlock), GenerateArgs = true, GenerateValidator = true, Namespace = "VK.Blocks.AI.Cognitive.Agents")]
public sealed partial record VKCognitiveAgentsOptions : IVKCognitiveAgentsOptions
{
    /// <inheritdoc />
    public bool Enabled { get; init; } = true;

    /// <inheritdoc />
    public string? DefaultPersonaId { get; init; }

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
}
