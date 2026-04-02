namespace VK.Blocks.Authentication.Features.Jwt.RefreshTokens;

/// <summary>
/// Service for managing JWT token revocations, ensuring robust logout operations.
/// </summary>
public interface IJwtTokenRevocationService
{
    #region Public Methods

    /// <summary>
    /// Revokes an access token asynchronously by its JWT ID (JTI).
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="jti">The JWT ID to revoke.</param>
    /// <param name="timeToLive">The optional time-to-live for the revocation entry.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous revocation operation.</returns>
    Task RevokeUserTokensAsync(string userId, string jti, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all future token validations for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="timeToLive">The optional time-to-live for the revocation entry.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous revocation operation.</returns>
    Task RevokeAllUserTokensAsync(string userId, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default);

    #endregion
}
