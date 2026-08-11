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
    private ImmutableList<VKPromptFragment> _evicted = ImmutableList<VKPromptFragment>.Empty;

    /// <summary>
    /// Gets all dialogue history fragments that were evicted/discarded.
    /// Lock-free, zero-allocation read access.
    /// </summary>
    public IReadOnlyList<VKPromptFragment> Evicted => _evicted;

    /// <summary>
    /// Adds a prompt fragment that was evicted.
    /// Uses CAS (Compare-And-Swap) for atomic, lock-free thread safety.
    /// </summary>
    /// <param name="fragment">The evicted fragment.</param>
    public void Add(VKPromptFragment fragment)
    {
        VKGuard.NotNull(fragment);
        ImmutableList<VKPromptFragment> initial, updated;
        do
        {
            initial = _evicted;
            updated = initial.Add(fragment);
        }
        while (Interlocked.CompareExchange(ref _evicted, updated, initial) != initial);
    }
}
