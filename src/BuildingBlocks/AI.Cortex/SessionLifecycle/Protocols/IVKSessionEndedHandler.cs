using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Pluggable extension contract for observing and handling session boundary termination events (e.g. Memory Consolidation).
/// Follows [AP.01], [CS.01], [CS.03].
/// </summary>
public interface IVKSessionEndedHandler
{
    /// <summary>
    /// Invoked when a session lifecycle boundary has ended.
    /// </summary>
    Task<VKResult> OnSessionEndedAsync(VKSessionId sessionId, CancellationToken cancellationToken = default);
}
