using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Persistence;

/// <summary>
/// Defines the contract for storing outbox messages within the same transaction as domain changes.
/// Ensures at-least-once delivery of domain events to external consumers.
/// </summary>
// TODO: Refactor Outbox architecture to separate concerns (DL.04):
// - Move message query, polling, retry, and processing (GetPendingAsync, MarkAsProcessedAsync) to a dedicated `VK.Blocks.Messaging.Outbox` building block.
// - Retain the message write contract (SaveAsync) in `VK.Blocks.Persistence` under a simplified `IVKOutboxWriter` interface.
public interface IVKOutboxStore
{
    /// <summary>
    /// Stores a domain event as an outbox message within the current transaction.
    /// </summary>
    // [CS.03]
    Task SaveAsync(VKOutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves pending outbox messages for processing.
    /// </summary>
    // [CS.03]
    Task<IReadOnlyList<VKOutboxMessage>> GetPendingAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified messages as processed.
    /// </summary>
    // [CS.03]
    Task MarkAsProcessedAsync(
        IReadOnlyList<Guid> messageIds,
        CancellationToken cancellationToken = default);
}
