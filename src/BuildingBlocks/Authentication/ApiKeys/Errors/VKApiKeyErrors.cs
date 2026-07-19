using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Errors associated with API key authentication.
/// </summary>
public static class VKApiKeyErrors
{
    /// <summary>
    /// VKError returned when the API key is empty or missing.
    /// </summary>
    public static readonly VKError Empty = new("ApiKey.Empty", "API key is empty", VKErrorType.Validation);

    /// <summary>
    /// VKError returned when the API key is invalid or not found.
    /// </summary>
    public static readonly VKError Invalid = new("ApiKey.Invalid", "Invalid API key", VKErrorType.Unauthorized);

    /// <summary>
    /// VKError returned when the API key has been revoked.
    /// </summary>
    public static readonly VKError Revoked = new("ApiKey.Revoked", "API key has been revoked", VKErrorType.Unauthorized);

    /// <summary>
    /// VKError returned when the API key has expired.
    /// </summary>
    public static readonly VKError Expired = new("ApiKey.Expired", "API key has expired", VKErrorType.Unauthorized);

    /// <summary>
    /// VKError returned when the API key is disabled.
    /// </summary>
    public static readonly VKError Disabled = new("ApiKey.Disabled", "API key is disabled", VKErrorType.Unauthorized);

    /// <summary>
    /// VKError returned when the API key has exceeded its rate limit.
    /// </summary>
    public static readonly VKError RateLimitExceeded = new("ApiKey.RateLimitExceeded", "Too many requests", VKErrorType.TooManyRequests);
}
