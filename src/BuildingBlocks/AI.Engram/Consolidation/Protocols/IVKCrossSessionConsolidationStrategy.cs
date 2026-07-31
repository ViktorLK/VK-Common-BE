using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Strategy contract for cross-session long-term memory replay consolidation.
/// </summary>
public interface IVKCrossSessionConsolidationStrategy
{
    /// <summary>
    /// Performs cross-session consolidation over sampled long-term memory entries.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKMemoryEntry>>> ConsolidateCrossSessionAsync(
        IReadOnlyList<VKMemoryEntry> sampledL3Memories,
        CancellationToken cancellationToken = default);
}
