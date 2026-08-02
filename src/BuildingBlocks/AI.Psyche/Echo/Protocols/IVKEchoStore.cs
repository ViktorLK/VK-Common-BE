using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain contract to track and sliding-window clean short-term memories.
/// Follows CS.01 and CS.03.
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
    /// Gets the optional parent session ID associated with a session for multi-level ancestry tracing.
    /// Default implementation returns null if parent relationship is not tracked.
    /// </summary>
    Task<VKResult<VKSessionId?>> GetParentSessionIdAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default) => Task.FromResult(VKResult.Success<VKSessionId?>(null));
}
