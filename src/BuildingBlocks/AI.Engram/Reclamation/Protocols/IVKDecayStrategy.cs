using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Defines a contract for calculating memory decay factor based on time and access frequency.
/// </summary>
public interface IVKDecayStrategy
{
    /// <summary>
    /// Evaluates updated RetentionScore for a batch of memory entries.
    /// </summary>
    /// <param name="entries">The entries to evaluate decay for.</param>
    /// <param name="options">The reclamation options containing half-life rules.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Updated memory entries with recalculated RetentionScore in metadata.</returns>
    Task<VKResult<IReadOnlyList<VKMemoryEntry>>> ApplyDecayAsync(
        IReadOnlyList<VKMemoryEntry> entries,
        VKReclamationOptions options,
        CancellationToken cancellationToken = default);
}
