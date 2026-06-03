using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel;

/// <summary>
/// Options for the Retrieval feature in Semantic Kernel.
/// </summary>
public sealed record VKRetrievalOptions : IVKRetrievalOptions, IVKAIProviderOptions, IVKAIGovernanceOptions, IVKToggleableBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => $"{VKAISKOptions.SectionName}:Retrieval";

    /// <summary>
    /// Gets or sets a value indicating whether the Retrieval feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <inheritdoc />
    public int? TopK { get; init; } = 5;

    /// <inheritdoc />
    public double? MinScore { get; init; } = 0.7;

    // --- Connection ---

    /// <inheritdoc />
    public VKAIProviderType? Provider { get; init; }

    /// <inheritdoc />
    public string? ModelId { get; init; }

    /// <inheritdoc />
    public VKSensitiveString? ApiKey { get; init; }

    /// <inheritdoc />
    public string? Endpoint { get; init; }

    // --- IVKAIGovernanceOptions ---

    /// <inheritdoc />
    public bool? EnableContentFilter { get; init; }

    /// <inheritdoc />
    public TimeSpan? Timeout { get; init; }

    /// <inheritdoc />
    public int? RetryCount { get; init; }

    /// <inheritdoc />
    public int? CircuitBreakerThreshold { get; init; }

    /// <inheritdoc />
    public TimeSpan? CircuitBreakerBreakDuration { get; init; }

    /// <inheritdoc />
    public bool? EnableAudit { get; init; }

    // --- IVKAIQuotaOptions ---

    /// <inheritdoc />
    public long? GlobalTokenLimit { get; init; }

    /// <inheritdoc />
    public long? MonthlyTokenBudget { get; init; }

    /// <inheritdoc />
    public int? RateLimitPerMinute { get; init; }

}
