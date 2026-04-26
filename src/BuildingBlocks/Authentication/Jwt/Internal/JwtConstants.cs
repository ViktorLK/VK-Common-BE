namespace VK.Blocks.Authentication.Jwt.Internal;

/// <summary>
/// Constants for JWT validation and configuration limits.
/// </summary>
internal static class JwtConstants
{
    /// <summary>
    /// The name of the Jwt feature.
    /// </summary>
    internal const string FeatureName = "Jwt";

    /// <summary>
    /// The minimum required length for the JWT secret key (32 characters / 256 bits).
    /// </summary>
    internal const int MinSecretKeyLength = 32;

    /// <summary>
    /// The maximum allowed expiration time for an access token in minutes (1440 minutes = 24 hours).
    /// </summary>
    internal const int MaxExpiryMinutes = 1440;

    /// <summary>
    /// The maximum allowed lifetime for a refresh token in days.
    /// </summary>
    internal const int MaxRefreshTokenLifetimeDays = 90;

    /// <summary>
    /// VKError message when DefaultScheme is missing.
    /// </summary>
    internal const string DefaultSchemeRequired = "DefaultScheme must be specified.";

    /// <summary>
    /// VKError message when Issuer is missing.
    /// </summary>
    internal const string IssuerRequired = "A valid JWT Issuer is required.";

    /// <summary>
    /// VKError message when Audience is missing.
    /// </summary>
    internal const string AudienceRequired = "A valid JWT Audience is required.";

    /// <summary>
    /// VKError message template when SecretKey is too short.
    /// </summary>
    internal const string SecretKeyLengthInvalid = "A JWT SecretKey is required and must be at least {0} characters long.";

    /// <summary>
    /// VKError message when Authority is missing for OIDC Discovery.
    /// </summary>
    internal const string AuthorityRequired = "Authority is required when AuthMode is set to OidcDiscovery.";

    /// <summary>
    /// VKError message for an invalid AuthMode configuration.
    /// </summary>
    internal const string InvalidAuthMode = "The configured AuthMode is invalid.";

    /// <summary>
    /// VKError message template for an invalid ExpiryMinutes range.
    /// </summary>
    internal const string ExpiryRangeInvalid = "ExpiryMinutes must be between 1 and {0} minutes.";

    /// <summary>
    /// VKError message template for an invalid RefreshTokenLifetimeDays range.
    /// </summary>
    internal const string RefreshTokenRangeInvalid = "RefreshTokenLifetimeDays must be between 1 and {0} days.";

    /// <summary>
    /// VKError message for an invalid ClockSkewSeconds value.
    /// </summary>
    internal const string ClockSkewInvalid = "ClockSkewSeconds must be zero or positive.";

    /// <summary>
    /// HTTP header name used to indicate that a token has expired.
    /// </summary>
    internal const string TokenExpiredHeader = "Token-Expired";

    /// <summary>
    /// Header value indicating true.
    /// </summary>
    internal const string HeaderTrueValue = "true";

    /// <summary>
    /// VKError detail message when a user session is revoked.
    /// </summary>
    internal const string UserRevokedDetail = "User session has been revoked.";

    /// <summary>
    /// VKError detail message when a specific token is revoked.
    /// </summary>
    internal const string TokenRevokedDetail = "Token has been revoked.";

    /// <summary>
    /// Default detail message for unauthorized responses.
    /// </summary>
    internal const string DefaultUnauthorizedDetail = "You are not authorized or the token is invalid.";
}
