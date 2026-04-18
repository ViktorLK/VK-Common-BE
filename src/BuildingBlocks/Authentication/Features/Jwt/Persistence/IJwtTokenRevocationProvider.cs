using System;
using System.Threading;
using System.Threading.Tasks;
namespace VK.Blocks.Authentication.Features.Jwt.Persistence;

/// <summary>
/// Manages revocation status for JWT IDs (JTI claims) whose tokens have been explicitly revoked.
/// </summary>
public interface IJwtTokenRevocationProvider
{
    /// <summary>
    /// Returns <c>true</c> when the given JTI has been revoked.
    /// </summary>
    /// <param name="jti">The JWT ID to check.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous check operation. The task result is <c>true</c> if the token is revoked; otherwise, <c>false</c>.</returns>
    ValueTask<bool> IsRevokedAsync(string jti, CancellationToken ct = default);

    /// <summary>
    /// Marks <paramref name="jti"/> as revoked for the specified <paramref name="ttl"/>.
    /// </summary>
    /// <param name="jti">The JWT ID to revoke.</param>
    /// <param name="ttl">The time-to-live for the revocation entry.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous revocation operation.</returns>
    ValueTask RevokeAsync(string jti, TimeSpan ttl, CancellationToken ct = default);

    /// <summary>
    /// Returns <c>true</c> if the entire user session has been revoked.
    /// </summary>
    /// <param name="userId">The user identifier to check.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous check operation.</returns>
    ValueTask<bool> IsUserRevokedAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Revokes all future token validations for the specified <paramref name="userId"/> for the given <paramref name="ttl"/>.
    /// </summary>
    /// <param name="userId">The user identifier whose tokens should be revoked.</param>
    /// <param name="ttl">The time-to-live for the revocation entry.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous revocation operation.</returns>
    ValueTask RevokeUserAsync(string userId, TimeSpan ttl, CancellationToken ct = default);
}
