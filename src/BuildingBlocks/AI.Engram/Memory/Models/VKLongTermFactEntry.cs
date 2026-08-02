using System;
using System.Collections.Generic;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Biological retention and decay metadata for an L3 long-term fact.
/// </summary>
public sealed record VKMemoryRetention
{
    /// <summary>
    /// Gets the evaluated importance score of the fact (0.0 to 1.0).
    /// </summary>
    public float Importance { get; init; } = 1.0f;

    /// <summary>
    /// Gets the current mathematical retention score after decay.
    /// </summary>
    public float RetentionScore { get; init; } = 1.0f;

    /// <summary>
    /// Gets the number of times this fact has been retrieved or accessed.
    /// </summary>
    public int AccessCount { get; init; }

    /// <summary>
    /// Gets the timestamp when the fact was last accessed.
    /// </summary>
    public DateTimeOffset? LastAccessedAt { get; init; }

    /// <summary>
    /// Gets the list of raw Echo trace IDs that this fact was distilled from (full traceability).
    /// </summary>
    public IReadOnlyList<Guid> SourceEchoIds { get; init; } = [];
}

/// <summary>
/// L3 LongTerm Fact Entry wrapping Psyche's <see cref="VKKnowledgeEntry"/> with Engram biological retention metadata.
/// Follows AP.01 and BB.01. Symmetry alignment with Corpus VKKnowledgeLifecycleEntry.
/// </summary>
public sealed record VKLongTermFactEntry
{
    /// <summary>
    /// Gets the inner Psyche knowledge entry fragment containing the fact text.
    /// </summary>
    public required VKKnowledgeEntry Knowledge { get; init; }

    /// <summary>
    /// Gets the retention, decay, and provenance traceability metadata.
    /// </summary>
    public required VKMemoryRetention Retention { get; init; }
}
