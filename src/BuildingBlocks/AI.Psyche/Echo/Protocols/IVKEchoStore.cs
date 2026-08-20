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
    /// Retrieves dialogue history for a given session.
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
}
