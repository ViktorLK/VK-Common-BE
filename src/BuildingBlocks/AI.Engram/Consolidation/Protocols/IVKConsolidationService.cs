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
    Task<VKResult> ConsolidateSessionMemoryAsync(VKPsycheContext context, CancellationToken cancellationToken = default);
}
