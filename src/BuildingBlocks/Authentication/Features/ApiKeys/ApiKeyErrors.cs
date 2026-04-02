using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.Features.ApiKeys;

/// <summary>
/// Errors associated with API key authentication.
/// </summary>
public static class ApiKeyErrors
{
    #region Fields

    /// <summary>
    /// Error returned when the API key is empty or missing.
    /// </summary>
    public static readonly Error Empty = new("ApiKey.Empty", "API key is empty", ErrorType.Validation);

    /// <summary>
    /// Error returned when the API key is invalid or not found.
    /// </summary>
    public static readonly Error Invalid = new("ApiKey.Invalid", "Invalid API key", ErrorType.Unauthorized);

    /// <summary>
    /// Error returned when the API key has been revoked.
    /// </summary>
    public static readonly Error Revoked = new("ApiKey.Revoked", "API key has been revoked", ErrorType.Unauthorized);

    /// <summary>
    /// Error returned when the API key has expired.
    /// </summary>
    public static readonly Error Expired = new("ApiKey.Expired", "API key has expired", ErrorType.Unauthorized);

    /// <summary>
    /// Error returned when the API key is disabled.
    /// </summary>
    public static readonly Error Disabled = new("ApiKey.Disabled", "API key is disabled", ErrorType.Unauthorized);

    /// <summary>
    /// Error returned when the API key has exceeded its rate limit.
    /// </summary>
    public static readonly Error RateLimitExceeded = new("ApiKey.RateLimitExceeded", "Too many requests", ErrorType.TooManyRequests);

    #endregion
}
