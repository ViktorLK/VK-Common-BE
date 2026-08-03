using System;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// L1 ShortTerm Memory Entry wrapping Psyche's <see cref="VKEchoTrace"/> with Engram lifecycle metadata.
/// Follows AP.01 and BB.01. Symmetry alignment with Corpus VKKnowledgeLifecycleEntry.
/// </summary>
public sealed record VKEchoEngramEntry
{
    /// <summary>
    /// Gets the inner Psyche echo trace fragment.
    /// </summary>
    public required VKEchoTrace Echo { get; init; }

    /// <summary>
    /// Gets the turn sequence index within the session.
    /// </summary>
    public int SequenceId { get; init; }

    /// <summary>
    /// Gets whether this echo has already been distilled into an L2 medium-term summary.
    /// </summary>
    public bool IsCompressed { get; init; }
}
