namespace VK.Blocks.Authentication.Features.Jwt;

/// <summary>
/// Defines a service for managing the revocation of JWT tokens.
/// </summary>
public interface IJwtTokenRevocationService
{
    /// <summary>
    /// Revokes an access token asynchronously by its JWT ID (JTI).
    /// </summary>
    /// <param name="userId">The unique identifier of the user (currently unused directly in revocation, but provided for context).</param>
    /// <param name="jti">The JWT ID to revoke.</param>
    /// <param name="timeToLive">The optional time-to-live for the revocation entry. Defaults to 1 day if not provided.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A task that represents the asynchronous revocation operation.</returns>
    Task RevokeUserTokensAsync(string userId, string jti, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all future token validations for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="timeToLive">The optional time-to-live for the revocation entry. Defaults to 7 days if not provided.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A task that represents the asynchronous revocation operation.</returns>
    Task RevokeAllUserTokensAsync(string userId, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default);
}
