using System;
using VK.Blocks.Authentication.ApiKeys.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Configuration settings for the API Key authentication.
/// </summary>
public sealed record VKApiKeyOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for ApiKey options.
    /// </summary>
    public static string SectionName => $"{VKAuthenticationOptions.SectionName}:{VKAuthenticationOptions.ApiKeySection}";

    /// <summary>
    /// Gets or sets a value indicating whether API key authentication is enabled.
    /// Defaults to false.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets or sets the authentication scheme name.
    /// Defaults to "ApiKey".
    /// </summary>
    public string SchemeName { get; init; } = ApiKeyConstants.DefaultAuthType;

    /// <summary>
    /// Gets or sets the header name used to pass the API key.
    /// Defaults to "X-Api-Key".
    /// </summary>
    public string HeaderName { get; init; } = ApiKeyConstants.DefaultHeaderName;

    /// <summary>
    /// Gets or sets the minimum required length for a raw API key.
    /// Defaults to 32.
    /// </summary>
    public int MinLength { get; init; } = 32;

    /// <summary>
    /// Gets or sets a value indicating whether rate limiting is enabled for API keys.
    /// </summary>
    public bool EnableRateLimiting { get; init; } = false;

    /// <summary>
    /// Gets or sets the maximum number of API key validations allowed per window.
    /// Defaults to 60.
    /// </summary>
    public int RateLimitPerMinute { get; init; } = 60;

    /// <summary>
    /// Gets or sets the window duration for rate limiting in seconds.
    /// Defaults to 60.
    /// </summary>
    public int RateLimitWindowSeconds { get; init; } = 60;

    /// <summary>
    /// Gets or sets the time-to-live for revocation records in the cache.
    /// Defaults to 5 minutes.
    /// </summary>
    public TimeSpan RevocationCacheTtl { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets a value indicating whether to track the last used timestamp for API keys.
    /// Defaults to false.
    /// </summary>
    public bool TrackLastUsedAt { get; init; } = false;
}
