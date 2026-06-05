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
    Task<VKResult<IReadOnlyCollection<VKEchoTrace>>> GetHistoryAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default);
}
