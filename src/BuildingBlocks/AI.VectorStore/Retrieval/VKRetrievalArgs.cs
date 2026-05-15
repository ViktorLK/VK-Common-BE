using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// Arguments for Retrieval execution.
/// Following AP.05: Hierarchical configuration pattern.
/// </summary>
public sealed record VKRetrievalArgs : IVKAIArgs, IVKRetrievalSettings, IVKGenerationSettings, IVKArgs<VKRetrievalArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKRetrievalArgs Empty { get; } = new();

    /// <inheritdoc />
    public IDictionary<string, object> Context { get; init; } = new Dictionary<string, object>();

    /// <inheritdoc />
    public VKAIProviderType? Provider { get; init; }

    /// <inheritdoc />
    public string? ModelId { get; init; }

    /// <inheritdoc />
    public TimeSpan? Timeout { get; init; }

    /// <inheritdoc />
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the tenant. Mandatory for multi-tenancy.
    /// </summary>
    public string? TenantId { get; init; }

    // Retrieval Parameters (IVKRetrievalSettings)

    /// <inheritdoc />
    public int? TopK { get; init; }

    /// <inheritdoc />
    public float? MinScore { get; init; }

    // Generation Parameters (IVKGenerationSettings)

    /// <inheritdoc />
    public float? Temperature { get; init; }

    /// <inheritdoc />
    public float? TopP { get; init; }

    /// <inheritdoc />
    public int? MaxTokens { get; init; }

    // RAG Specific Parameters

    /// <summary>
    /// Gets the collection name to search in.
    /// </summary>
    public string? Collection { get; init; }

    /// <summary>
    /// Gets a value indicating whether to apply temporal weighting (memory decay).
    /// </summary>
    public bool EnableTemporalWeighting { get; init; } = false;

    /// <summary>
    /// Gets the decay rate for temporal weighting (higher values mean faster decay).
    /// </summary>
    public double DecayRate { get; init; } = 0.1;
}
