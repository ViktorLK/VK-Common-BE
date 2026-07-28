using System.Collections.Generic;
using System.Collections.Frozen;
using VK.Blocks.Core;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Options and criteria for searching AI memories.
/// </summary>
public sealed record VKMemoryQuery
{
    /// <summary>
    /// Gets the optional tenant identifier for tenant-isolated memory retrieval (Framework-level boundary).
    /// </summary>
    public VKTenantId? TenantId { get; init; }

    /// <summary>
    /// Gets the optional session identifier filter for session-scoped memory retrieval.
    /// </summary>
    public VKSessionId? SessionId { get; init; }

    /// <summary>
    /// Gets the semantic text query to match against memory content.
    /// </summary>
    public string? SemanticQuery { get; init; }

    /// <summary>
    /// Gets the optional category filter.
    /// </summary>
    public VKMemoryCategory? Category { get; init; }

    /// <summary>
    /// Gets the immutable extended scope dictionary (e.g. SessionId, UserId, PersonaId, DeviceId, etc.).
    /// Store implementations filter entries by exact matching these scope key-values.
    /// </summary>
    public IReadOnlyDictionary<string, string> ExtendedScope { get; init; } = FrozenDictionary<string, string>.Empty;

    /// <summary>
    /// Gets the maximum number of memories to retrieve.
    /// </summary>
    public int TopK { get; init; } = 5;

    /// <summary>
    /// Gets the minimum relevance score threshold.
    /// </summary>
    public float MinScore { get; init; } = 0.7f;

    /// <summary>
    /// Gets a value indicating whether to enable temporal weighting (decay over time).
    /// </summary>
    public bool EnableTemporalWeighting { get; init; }

    /// <summary>
    /// Gets the custom decay rate.
    /// </summary>
    public double? DecayRate { get; init; }
}
