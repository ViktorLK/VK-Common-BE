using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Defines a contract for evaluating contradiction between new facts and existing memories.
/// </summary>
public interface IVKContradictionArbitrator
{
    /// <summary>
    /// Arbitrates whether a new memory fact contradicts existing memory entries.
    /// </summary>
    /// <param name="newFact">The new memory fact text.</param>
    /// <param name="existingCandidates">The candidate existing memories to evaluate against.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Arbitration result containing contradiction kind and target memory ID.</returns>
    Task<VKResult<VKContradictionArbitrationResult>> ArbitrateAsync(
        string newFact,
        IReadOnlyList<VKMemoryEntry> existingCandidates,
        CancellationToken cancellationToken = default);
}
