using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using VK.Blocks.Core;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Infrastructure Persistence DTO: Represents a static entry in the AI Memory Store.
/// <para>
/// This is the lowest-level physical storage model. Unlike a <see cref="VKMemoryTrace"/>,
/// an Entry is mathematically cold—it does not decay and has no biological activation.
/// It serves purely as the immutable, objective audit log of what actually happened.
/// </para>
/// </summary>
public sealed record VKMemoryEntry
{
    /// <summary>
    /// Gets the unique identifier for the memory entry.
    /// </summary>
    public required VKMemoryId Id { get; init; }

    /// <summary>
    /// Gets the optional strongly-typed tenant identifier associated with the memory entry for strict isolation.
    /// </summary>
    public VKTenantId? TenantId { get; init; }

    /// <summary>
    /// Gets the optional strongly-typed chat session identifier for L1 (ShortTerm) and L2 (MediumTerm) scope isolation.
    /// </summary>
    public VKSessionId? SessionId { get; init; }

    /// <summary>
    /// Gets the content/text of the memory.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the category of the memory, determining its life-cycle strategy.
    /// </summary>
    public VKMemoryCategory Category { get; init; } = VKMemoryCategory.ShortTerm;

    /// <summary>
    /// Gets the immutable extended scope dictionary (e.g. SessionId, UserId, PersonaId, DeviceId, etc.).
    /// Store implementations filter entries against exact matching scope key-values.
    /// </summary>
    public IReadOnlyDictionary<string, string> ExtendedScope { get; init; } = FrozenDictionary<string, string>.Empty;

    /// <summary>
    /// Gets the metadata associated with the memory.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = FrozenDictionary<string, string>.Empty;

    /// <summary>
    /// Gets the timestamp when the memory was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the importance score of the memory (0.0 to 1.0).
    /// </summary>
    public float Importance { get; init; } = 1.0f;

    /// <summary>
    /// Gets the timestamp when the memory was last accessed or retrieved.
    /// </summary>
    public DateTimeOffset? LastAccessedAt { get; init; }

    /// <summary>
    /// Gets the emotional tagging information of the memory, if any.
    /// </summary>
    public VKEmotionalSignal? Emotion { get; init; }

    /// <summary>
    /// Gets a value indicating whether this memory entry is pinned/protected from pruning or compression.
    /// </summary>
    public bool IsPinned { get; init; }

    /// <summary>
    /// Gets the optional hard time-to-live threshold. Once elapsed, the entry is expired regardless of RetentionScore.
    /// </summary>
    public TimeSpan? HardTtl { get; init; }

    /// <summary>
    /// Gets the optional BCP-47 language tag of the memory entry (e.g., "en-US", "ja-JP").
    /// </summary>
    public string? LanguageCode { get; init; }

    /// <summary>
    /// Gets the version index of this memory entry, incremented on each revision update (starts at 1).
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Gets a value indicating whether this memory entry has been contradicted and superseded by a newer fact.
    /// </summary>
    public bool IsSuperseded { get; init; }

    /// <summary>
    /// Gets the ID of the newer memory entry that superseded this memory entry, if any.
    /// </summary>
    public VKMemoryId? SupersededBy { get; init; }
}
