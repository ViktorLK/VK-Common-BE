using System;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Authentication.Abstractions;

/// <summary>
/// Manages a deny-list of JWT IDs (JTI claims) whose tokens have been explicitly revoked
/// before their natural expiry (e.g., logout, forced sign-out, stolen-token recovery).
/// </summary>
public interface ITokenBlacklist
{
    /// <summary>
    /// Returns <c>true</c> when the given JTI has been added to the deny-list,
    /// meaning the corresponding access token must be rejected even if it is
    /// otherwise cryptographically valid and not yet expired.
    /// </summary>
    /// <param name="jti">The JWT ID to check.</param>
    /// <param name="ct">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A task that represents the asynchronous check operation. The task result is <c>true</c> if the token is revoked; otherwise, <c>false</c>.</returns>
    Task<bool> IsRevokedAsync(string jti, CancellationToken ct = default);

    /// <summary>
    /// Adds <paramref name="jti"/> to the deny-list for the specified <paramref name="ttl"/>.
    /// Set <paramref name="ttl"/> to the token's remaining lifetime so the entry is
    /// automatically evicted once the token would have expired anyway.
    /// </summary>
    /// <param name="jti">The JWT ID to revoke.</param>
    /// <param name="ttl">The time-to-live for the revocation entry.</param>
    /// <param name="ct">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A task that represents the asynchronous revocation operation.</returns>
    Task RevokeAsync(string jti, TimeSpan ttl, CancellationToken ct = default);
}
