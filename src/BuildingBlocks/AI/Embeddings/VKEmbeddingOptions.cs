using System;
using VK.Blocks.AI.Embeddings.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Embeddings feature.
/// </summary>
public sealed record VKEmbeddingOptions : IVKAIProviderSettings, IVKAIGovernanceSettings, IVKToggleableBlockOptions
{
    /// <summary>
    /// The configuration section name for Embeddings options.
    /// </summary>
    public static string SectionName => $"{VKAIOptions.SectionName}:{EmbeddingsConstants.FeatureName}";

    /// <summary>
    /// Gets or sets a value indicating whether Embeddings feature is enabled.
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

    // --- Embedding Specific ---

    /// <summary>
    /// Gets or sets the dimensions of the embedding vectors.
    /// </summary>
    public int? Dimensions { get; init; }

    /// <summary>
    /// Gets or sets the batch size for embedding generation.
    /// </summary>
    public int BatchSize { get; init; } = 16;
}
