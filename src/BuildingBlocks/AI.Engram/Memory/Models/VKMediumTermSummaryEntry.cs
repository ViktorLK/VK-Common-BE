using System;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Lineage and rolling window metadata associated with an L2 medium-term summary.
/// </summary>
public sealed record VKSummaryLineage
{
    /// <summary>
    /// Gets the starting timestamp covered by this L2 summary.
    /// </summary>
    public DateTimeOffset StartTimestamp { get; init; }

    /// <summary>
    /// Gets the ending timestamp covered by this L2 summary (next compression picks up messages > EndTimestamp).
    /// </summary>
    public DateTimeOffset EndTimestamp { get; init; }

    /// <summary>
    /// Gets the parent L2 summary ID that this summary incremented from.
    /// </summary>
    public VKKnowledgeId? ParentSummaryId { get; init; }

    /// <summary>
    /// Gets whether this L2 summary is the currently active/latest version for prompt weaving.
    /// </summary>
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// L2 MediumTerm Summary Entry wrapping Psyche's <see cref="VKKnowledgeEntry"/> with Engram rolling window lineage.
/// Follows AP.01 and BB.01. Symmetry alignment with Corpus VKKnowledgeLifecycleEntry.
/// </summary>
public sealed record VKMediumTermSummaryEntry
{
    /// <summary>
    /// Gets the inner Psyche knowledge entry fragment containing the summary text.
    /// </summary>
    public required VKKnowledgeEntry Knowledge { get; init; }

    /// <summary>
    /// Gets the rolling window lineage and version control metadata.
    /// </summary>
    public required VKSummaryLineage Lineage { get; init; }
}
