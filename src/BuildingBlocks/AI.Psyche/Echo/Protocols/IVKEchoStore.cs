using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain contract to track and sliding-window clean short-term memories.
/// Follows CS.01 and CS.03 patterns.
/// </summary>
public interface IVKEchoStore
{
    /// <summary>
    /// Phase 1: Retrieves lightweight dialogue metadata for a given session without full message content.
    /// Ordered chronologically from oldest to newest.
    /// </summary>
    Task<VKResult<IReadOnlyCollection<VKEchoMetadata>>> GetMetadataAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Phase 2: Retrieves full dialogue traces for the specified list of echo trace identifiers.
    /// Ordered chronologically from oldest to newest.
    /// </summary>
    Task<VKResult<IReadOnlyCollection<VKEchoTrace>>> GetTracesByIdsAsync(
        IReadOnlyCollection<VKEchoId> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves full dialogue history for a given session.
    /// </summary>
    Task<VKResult<IReadOnlyCollection<VKEchoTrace>>> GetHistoryAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends a new conversation echo trace to short-term memory.
    /// </summary>
    Task<VKResult> SaveHistoryAsync(
        VKEchoTrace trace,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends multiple conversation echo traces to short-term memory in a single batch.
    /// </summary>
    Task<VKResult> SaveHistoryBatchAsync(
        IReadOnlyCollection<VKEchoTrace> traces,
        CancellationToken cancellationToken = default);
}
