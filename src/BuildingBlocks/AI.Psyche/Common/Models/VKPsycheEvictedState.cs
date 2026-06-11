using System.Collections.Generic;
using System.Threading;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// State container holding prompt fragments evicted/discarded during execution.
/// Complies with AP.01 (sealed class).
/// </summary>
public sealed class VKPsycheEvictedState
{
    private readonly Lock _lock = new();
    private readonly List<VKPromptFragment> _evicted = [];

    /// <summary>
    /// Gets all dialogue history fragments that were evicted/discarded.
    /// </summary>
    public IReadOnlyList<VKPromptFragment> Evicted
    {
        get
        {
            lock (_lock)
            {
                return [.. _evicted];
            }
        }
    }

    /// <summary>
    /// Adds a prompt fragment that was evicted.
    /// </summary>
    /// <param name="fragment">The evicted fragment.</param>
    public void Add(VKPromptFragment fragment)
    {
        VKGuard.NotNull(fragment);
        lock (_lock)
        {
            _evicted.Add(fragment);
        }
    }
}
