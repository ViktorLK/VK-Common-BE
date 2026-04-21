using System;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Authentication.Jwt.Internal;

/// <summary>
/// Manages the revocation of JWT tokens, ensuring users are fully logged out from their JWT sessions.
/// </summary>
internal sealed class JwtTokenRevocationService(IVKJwtRevocationProvider revocationProvider) : IVKJwtRevocationService
{
    /// <summary>
    /// Revokes an access token asynchronously by its JWT ID (JTI).
    /// </summary>
    /// <param name="userId">The unique identifier of the user (currently unused directly in revocation, but provided for context).</param>
    /// <param name="jti">The JWT ID to revoke.</param>
    /// <param name="timeToLive">The optional time-to-live for the revocation entry. Defaults to 1 day if not provided.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A task that represents the asynchronous revocation operation.</returns>
    public async Task RevokeUserTokensAsync(string userId, string jti, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default)
    {
        // Add AccessToken's JTI to Blacklist
        if (!string.IsNullOrEmpty(jti))
        {
            var ttl = timeToLive ?? TimeSpan.FromDays(1);
            await revocationProvider.RevokeAsync(jti, ttl, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Revokes all future token validations for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="timeToLive">The optional time-to-live for the revocation entry. Defaults to 7 days if not provided.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A task that represents the asynchronous revocation operation.</returns>
    public async Task RevokeAllUserTokensAsync(string userId, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(userId))
        {
            var ttl = timeToLive ?? TimeSpan.FromDays(7);
            await revocationProvider.RevokeUserAsync(userId, ttl, cancellationToken).ConfigureAwait(false);
        }
    }
}
