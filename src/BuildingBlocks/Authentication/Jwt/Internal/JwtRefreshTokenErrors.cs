using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Jwt.Internal;

/// <summary>
/// Errors associated with JWT Refresh Token operations.
/// </summary>
internal static class JwtRefreshTokenErrors
{
    /// <summary>
    /// VKError when any required identifier (JTI or Family ID) is missing.
    /// </summary>
    public static readonly VKError InvalidIds = new("RefreshToken.InvalidIds", "The token JTI or family ID is missing.", VKErrorType.Validation);

    /// <summary>
    /// VKError when a refresh token replay/reuse is detected (Compromised security).
    /// </summary>
    public static readonly VKError Compromised = new("RefreshToken.Compromised", "The refresh token has already been consumed. Replay attack detected.", VKErrorType.Unauthorized);

    /// <summary>
    /// VKError when the refresh token has expired.
    /// </summary>
    public static readonly VKError Expired = new("RefreshToken.Expired", "The refresh token has expired.", VKErrorType.Unauthorized);

    /// <summary>
    /// VKError when a server-side service (like Redis) is unavailable.
    /// </summary>
    public static readonly VKError ServiceUnavailable = new("RefreshToken.ServiceUnavailable", "The refresh token server is currently unavailable.", VKErrorType.Failure);
}
