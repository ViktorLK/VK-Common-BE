using System;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Defines a contract for handling failed consolidation entries (Dead-Letter Queue hook).
/// Allows external message bus integration (e.g. Messaging module bridge) to capture failed memories.
/// </summary>
public interface IVKConsolidationDlqHandler
{
    /// <summary>
    /// Handles a memory entry that failed to consolidate/persist after retries.
    /// </summary>
    /// <param name="entry">The memory entry that failed to persist.</param>
    /// <param name="exception">The exception associated with the failure, if any.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the async DLQ handling operation.</returns>
    ValueTask HandleFailedEntryAsync(
        VKMemoryEntry entry,
        Exception? exception = null,
        CancellationToken cancellationToken = default);
}
