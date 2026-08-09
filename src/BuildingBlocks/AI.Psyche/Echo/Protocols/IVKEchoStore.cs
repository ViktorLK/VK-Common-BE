using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain contract to track and sliding-window clean short-term memories.
/// Follows CS.01, CS.03, and Ambient Context isolation patterns.
/// Stores automatically resolve TenantId and UserId via injected <see cref="IVKIdentityContext"/>.
/// </summary>
public interface IVKEchoStore
{
    /// <summary>
    /// Retrieves dialogue history for a given session within the current ambient identity context.
    /// </summary>
    Task<VKResult<IReadOnlyCollection<VKEchoTrace>>> GetHistoryAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends a new conversation echo trace to short-term memory.
    /// </summary>
    Task<VKResult> SaveTraceAsync(
        VKEchoTrace trace,
        CancellationToken cancellationToken = default);
}
