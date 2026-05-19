using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the storage contract for tracking session-level runtime turn timers and cooldown states for knowledge entries.
/// </summary>
public interface IVKKnowledgeSessionStateStore
{
    /// <summary>
    /// Retrieves the runtime session state for a specific knowledge entry in a session.
    /// </summary>
    Task<VKResult<VKKnowledgeSessionState?>> GetStateAsync(
        string sessionId,
        string knowledgeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves or updates the runtime session state for a knowledge entry in a session.
    /// </summary>
    Task<VKResult> SaveStateAsync(
        VKKnowledgeSessionState state,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active session states for a specific session.
    /// </summary>
    Task<VKResult<IEnumerable<VKKnowledgeSessionState>>> GetSessionStatesAsync(
        string sessionId,
        CancellationToken cancellationToken = default);
}
