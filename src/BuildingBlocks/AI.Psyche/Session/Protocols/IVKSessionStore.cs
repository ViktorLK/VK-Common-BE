using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain contract to manage session thread metadata, lifecycle, and lineage.
/// Follows CS.01 and CS.03.
/// </summary>
public interface IVKSessionStore
{
    /// <summary>
    /// Retrieves a session thread by its unique session ID.
    /// </summary>
    Task<VKResult<VKSessionThread?>> GetSessionAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the currently active session thread for a given persona and user boundary.
    /// </summary>
    Task<VKResult<VKSessionThread?>> GetActiveSessionAsync(
        VKPersonaId personaId,
        VKUserId? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts (creates or updates) a session thread.
    /// </summary>
    Task<VKResult> SaveSessionAsync(
        VKSessionThread session,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronously performs a lightweight activity touch on the session (updates LastActivityAt & increments TurnCount).
    /// </summary>
    Task<VKResult> TouchSessionAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default);
}
