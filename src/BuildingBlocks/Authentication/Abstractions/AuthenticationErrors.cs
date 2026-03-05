using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.Abstractions;

/// <summary>
/// Static class containing standard errors used throughout the Authentication module.
/// </summary>
public static class AuthenticationErrors
{
    public static class Jwt
    {
        public static readonly Error EmptyToken = new("Auth.EmptyToken", "The provided token is empty.", ErrorType.Validation);
        public static readonly Error ConfigurationError = new("Auth.ConfigurationError", "Authentication is not configured.", ErrorType.Failure);
        public static readonly Error InvalidFormat = new("Auth.InvalidFormat", "The provided token is not a valid JWT.", ErrorType.Validation);
        public static readonly Error Revoked = new("Auth.Revoked", "The token has been revoked.", ErrorType.Unauthorized);
        public static readonly Error Expired = new("Auth.Expired", "The token has expired.", ErrorType.Unauthorized);
        public static readonly Error Invalid = new("Auth.Invalid", "The token is invalid.", ErrorType.Unauthorized);
    }

    public static class RefreshToken
    {
        public static readonly Error InvalidIds = new("RefreshToken.InvalidIds", "Token JTI or Family ID cannot be empty", ErrorType.Validation);
        public static readonly Error Compromised = new("RefreshToken.Compromised", "Token rotation violation detected. The token family may be compromised.", ErrorType.Unauthorized);
    }

    public static class ApiKey
    {
        public static readonly Error Empty = new("ApiKey.Empty", "API key is empty", ErrorType.Validation);
        public static readonly Error Invalid = new("ApiKey.Invalid", "Invalid API key", ErrorType.Unauthorized);
        public static readonly Error Revoked = new("ApiKey.Revoked", "API key has been revoked", ErrorType.Unauthorized);
        public static readonly Error Expired = new("ApiKey.Expired", "API key has expired", ErrorType.Unauthorized);
        public static readonly Error Disabled = new("ApiKey.Disabled", "API key is disabled", ErrorType.Unauthorized);
        public static readonly Error RateLimitExceeded = new("ApiKey.RateLimitExceeded", "Too many requests", ErrorType.Validation);
    }
}
