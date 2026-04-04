using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.Features.Jwt.RefreshTokens;

/// <summary>
/// Errors associated with JWT Refresh Token operations.
/// </summary>
public static class JwtRefreshTokenErrors
{
    #region Fields

    /// <summary>
    /// Error when any required identifier (JTI or Family ID) is missing.
    /// </summary>
    public static readonly Error InvalidIds = new("RefreshToken.InvalidIds", "The token JTI or family ID is missing.", ErrorType.Validation);

    /// <summary>
    /// Error when a refresh token replay/reuse is detected (Compromised security).
    /// </summary>
    public static readonly Error Compromised = new("RefreshToken.Compromised", "The refresh token has already been consumed. Replay attack detected.", ErrorType.Unauthorized);

    /// <summary>
    /// Error when the refresh token has expired.
    /// </summary>
    public static readonly Error Expired = new("RefreshToken.Expired", "The refresh token has expired.", ErrorType.Unauthorized);

    /// <summary>
    /// Error when a server-side service (like Redis) is unavailable.
    /// </summary>
    public static readonly Error ServiceUnavailable = new("RefreshToken.ServiceUnavailable", "The refresh token server is currently unavailable.", ErrorType.Failure);

    #endregion
}
