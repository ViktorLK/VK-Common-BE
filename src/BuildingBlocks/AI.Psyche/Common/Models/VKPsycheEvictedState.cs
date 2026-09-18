using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// State container holding prompt fragments evicted/discarded during execution.
/// Complies with AP.01 (sealed class).
/// Uses ImmutableList with CAS for lock-free, zero-allocation thread safety.
/// </summary>
public sealed class VKPsycheEvictedState
{
    private ImmutableList<VKPromptSegment> _evictedSegments = [];
    private ImmutableList<VKEchoFragment> _evictedEchoes = [];

    /// <summary>
    /// Gets all static prompt segments (e.g. Directive, Persona, Knowledge) that were evicted/discarded.
    /// Lock-free, zero-allocation read access.
    /// </summary>
    public IReadOnlyList<VKPromptSegment> EvictedSegments => _evictedSegments;

    /// <summary>
    /// Gets all dialogue echo turns that were evicted during token budget truncation.
    /// Preserves original turn metadata including role, timestamps, and turn index.
    /// </summary>
    public IReadOnlyList<VKEchoFragment> EvictedEchoes => _evictedEchoes;

    /// <summary>
    /// Backward-compatibility alias for <see cref="EvictedSegments"/>.
    /// </summary>
    public IReadOnlyList<VKPromptSegment> Evicted => _evictedSegments;

    /// <summary>
    /// Gets the total count of all evicted segments and echoes combined.
    /// </summary>
    public int TotalEvictedCount => _evictedSegments.Count + _evictedEchoes.Count;

    /// <summary>
    /// Adds a prompt segment that was evicted.
    /// Uses CAS (Compare-And-Swap) for atomic, lock-free thread safety.
    /// </summary>
    /// <param name="segment">The evicted segment.</param>
    public void Add(VKPromptSegment segment)
    {
        VKGuard.NotNull(segment);
        ImmutableList<VKPromptSegment> initial, updated;
        do
        {
            initial = _evictedSegments;
            updated = initial.Add(segment);
        }
        while (Interlocked.CompareExchange(ref _evictedSegments, updated, initial) != initial);
    }

    /// <summary>
    /// Adds an echo fragment that was evicted during token budget truncation.
    /// Preserves full echo metadata in EvictedEchoes.
    /// </summary>
    /// <param name="echo">The evicted echo fragment.</param>
    public void Add(VKEchoFragment echo)
    {
        VKGuard.NotNull(echo);
        ImmutableList<VKEchoFragment> initialEchoes, updatedEchoes;
        do
        {
            initialEchoes = _evictedEchoes;
            updatedEchoes = initialEchoes.Add(echo);
        }
        while (Interlocked.CompareExchange(ref _evictedEchoes, updatedEchoes, initialEchoes) != initialEchoes);
    }
}
