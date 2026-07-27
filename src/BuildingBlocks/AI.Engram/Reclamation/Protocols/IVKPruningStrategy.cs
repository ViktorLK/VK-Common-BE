using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Defines a contract for evaluating pruning actions on low retention memories.
/// </summary>
public interface IVKPruningStrategy
{
    /// <summary>
    /// Evaluates which memory entries fall below pruning thresholds.
    /// </summary>
    /// <param name="entries">The candidate memory entries to evaluate.</param>
    /// <param name="options">The reclamation options with category/persona thresholds.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Mapping of memory entries to assigned pruning actions.</returns>
    Task<VKResult<IReadOnlyDictionary<VKMemoryId, VKPruneAction>>> EvaluatePruningAsync(
        IReadOnlyList<VKMemoryEntry> entries,
        VKReclamationOptions options,
        CancellationToken cancellationToken = default);
}
