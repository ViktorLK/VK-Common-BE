using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Consolidation;

/// <summary>
/// Service coordinating the consolidation of L2 session memories into L3 persistent memory.
/// </summary>
public interface IVKConsolidationService
{
    /// <summary>
    /// Processes the Psyche context to consolidate memory from the current round into long-term storage.
    /// </summary>
    Task<VKResult> ConsolidateSessionMemoryAsync(VKPsycheContext context, VKConsolidationArgs? args = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes long-term consolidation for a specific session ID out-of-band.
    /// </summary>
    Task<VKResult> ConsolidateSessionMemoryAsync(VKSessionId sessionId, VKConsolidationArgs? args = null, CancellationToken cancellationToken = default);
}
