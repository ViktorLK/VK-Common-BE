using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Defines a contract for tracking and recording access events on memory entries.
/// </summary>
public interface IVKAccessTracker
{
    /// <summary>
    /// Records an access event for a single memory entry, updating LastAccessedAt and incrementing AccessCount.
    /// </summary>
    Task<VKResult> RecordAccessAsync(VKMemoryId memoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records access events for a batch of memory entries.
    /// </summary>
    Task<VKResult> RecordAccessBatchAsync(IEnumerable<VKMemoryId> memoryIds, CancellationToken cancellationToken = default);
}
