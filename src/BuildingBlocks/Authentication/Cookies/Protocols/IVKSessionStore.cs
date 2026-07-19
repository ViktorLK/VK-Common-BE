using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Cookies.Protocols;

/// <summary>
/// Defines a contract for registering and managing user sessions.
/// </summary>
public interface IVKSessionStore
{
    /// <summary>
    /// Registers a new session, enforcing concurrent session limits if configured.
    /// </summary>
    /// <param name="userId">The unique user identifier.</param>
    /// <param name="ticketData">The serialized authentication ticket data.</param>
    /// <param name="expiresAt">The date and time when the session expires.</param>
    /// <param name="maxConcurrentSessions">The maximum allowed concurrent sessions.</param>
    /// <param name="kickOldest">Whether to kick the oldest session or reject the new one.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result containing the unique session identifier if successful.</returns>
    ValueTask<VKResult<string>> RegisterSessionAsync(
        string userId,
        string ticketData,
        DateTimeOffset expiresAt,
        int maxConcurrentSessions,
        bool kickOldest,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves the serialized ticket data for a session if it is valid.
    /// </summary>
    /// <param name="sessionId">The unique session identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The ticket data if found and valid; otherwise, null.</returns>
    ValueTask<string?> GetSessionTicketAsync(string sessionId, CancellationToken ct = default);

    /// <summary>
    /// Updates the session ticket data and expiration time.
    /// </summary>
    /// <param name="sessionId">The unique session identifier.</param>
    /// <param name="ticketData">The updated ticket data.</param>
    /// <param name="expiresAt">The updated expiration time.</param>
    /// <param name="ct">The cancellation token.</param>
    ValueTask UpdateSessionTicketAsync(string sessionId, string ticketData, DateTimeOffset expiresAt, CancellationToken ct = default);

    /// <summary>
    /// Revokes the specified session.
    /// </summary>
    /// <param name="sessionId">The unique session identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    ValueTask RevokeSessionAsync(string sessionId, CancellationToken ct = default);

    /// <summary>
    /// Checks whether the specified session is active.
    /// </summary>
    /// <param name="sessionId">The unique session identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if the session exists and has not expired; otherwise, false.</returns>
    ValueTask<bool> IsSessionActiveAsync(string sessionId, CancellationToken ct = default);

    /// <summary>
    /// Revokes all active sessions for the specified user.
    /// </summary>
    /// <param name="userId">The unique user identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    ValueTask RevokeUserSessionsAsync(string userId, CancellationToken ct = default);
}
