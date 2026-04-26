namespace VK.Blocks.Authentication.ApiKeys.Internal;

/// <summary>
/// Contains constants used by the ApiKeys infrastructure.
/// </summary>
internal static class ApiKeyConstants
{
    /// <summary>
    /// The name of the ApiKey feature.
    /// </summary>
    internal const string FeatureName = "ApiKey";

    /// <summary>
    /// The default HTTP header name for API keys.
    /// </summary>
    internal const string DefaultHeaderName = "X-Api-Key";

    /// <summary>
    /// The default authentication type for API keys.
    /// </summary>
    internal const string DefaultAuthType = "ApiKey";

    // Validation Messages
    internal const string HeaderNameRequired = "A valid HeaderName is required for API Key authentication.";
    internal const string SchemeNameRequired = "A valid SchemeName is required for API Key authentication.";
    internal const string MinLengthInvalid = "ApiKey:MinLength must be a non-negative integer.";
    internal const string RateLimitPerMinuteInvalid = "ApiKey:RateLimitPerMinute must be greater than 0 when EnableRateLimiting is true.";
    internal const string RateLimitWindowSecondsInvalid = "ApiKey:RateLimitWindowSeconds must be greater than 0 when EnableRateLimiting is true.";
    internal const string UnauthorizedMessage = "API key is missing or invalid";
}
