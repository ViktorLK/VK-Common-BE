namespace VK.Blocks.Authentication.Features.Jwt;

/// <summary>
/// Constants for JWT validation and configuration limits.
/// </summary>
internal static class JwtConstants
{
    #region Fields

    /// <summary>
    /// The minimum required length for the JWT secret key (32 characters / 256 bits).
    /// </summary>
    public const int MinSecretKeyLength = 32;

    /// <summary>
    /// The maximum allowed expiration time for an access token in minutes (1440 minutes = 24 hours).
    /// </summary>
    public const int MaxExpiryMinutes = 1440;

    /// <summary>
    /// The maximum allowed lifetime for a refresh token in days.
    /// </summary>
    public const int MaxRefreshTokenLifetimeDays = 90;

    #region Configuration Validation

    /// <summary>
    /// Error message when DefaultScheme is missing.
    /// </summary>
    public const string DefaultSchemeRequired = "DefaultScheme must be specified.";

    /// <summary>
    /// Error message when Issuer is missing.
    /// </summary>
    public const string IssuerRequired = "A valid JWT Issuer is required.";

    /// <summary>
    /// Error message when Audience is missing.
    /// </summary>
    public const string AudienceRequired = "A valid JWT Audience is required.";

    /// <summary>
    /// Error message template when SecretKey is too short.
    /// </summary>
    public const string SecretKeyLengthInvalid = "A JWT SecretKey is required and must be at least {0} characters long.";

    /// <summary>
    /// Error message when Authority is missing for OIDC Discovery.
    /// </summary>
    public const string AuthorityRequired = "Authority is required when AuthMode is set to OidcDiscovery.";

    /// <summary>
    /// Error message for an invalid AuthMode configuration.
    /// </summary>
    public const string InvalidAuthMode = "The configured AuthMode is invalid.";

    /// <summary>
    /// Error message template for an invalid ExpiryMinutes range.
    /// </summary>
    public const string ExpiryRangeInvalid = "ExpiryMinutes must be between 1 and {0} minutes.";

    /// <summary>
    /// Error message template for an invalid RefreshTokenLifetimeDays range.
    /// </summary>
    public const string RefreshTokenRangeInvalid = "RefreshTokenLifetimeDays must be between 1 and {0} days.";

    /// <summary>
    /// Error message for an invalid ClockSkewSeconds value.
    /// </summary>
    public const string ClockSkewInvalid = "ClockSkewSeconds must be zero or positive.";

    #endregion

    #region Response Headers & Details

    /// <summary>
    /// HTTP header name used to indicate that a token has expired.
    /// </summary>
    public const string TokenExpiredHeader = "Token-Expired";

    /// <summary>
    /// Header value indicating true.
    /// </summary>
    public const string HeaderTrueValue = "true";

    /// <summary>
    /// Error detail message when a user session is revoked.
    /// </summary>
    public const string UserRevokedDetail = "User session has been revoked.";

    /// <summary>
    /// Error detail message when a specific token is revoked.
    /// </summary>
    public const string TokenRevokedDetail = "Token has been revoked.";

    /// <summary>
    /// Default detail message for unauthorized responses.
    /// </summary>
    public const string DefaultUnauthorizedDetail = "You are not authorized or the token is invalid.";

    #endregion

    #endregion
}
