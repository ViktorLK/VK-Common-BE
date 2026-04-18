using System;
namespace VK.Blocks.Authentication.Features.ApiKeys;

/// <summary>
/// Configuration settings for the API Key authentication.
/// </summary>
public sealed class ApiKeyOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether API key authentication is enabled.
    /// Defaults to false.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the authentication scheme name.
    /// Defaults to "ApiKey".
    /// </summary>
    public string SchemeName { get; set; } = "ApiKey";

    /// <summary>
    /// Gets or sets the header name used to pass the API key.
    /// Defaults to "X-Api-Key".
    /// </summary>
    public string HeaderName { get; set; } = "X-Api-Key";

    /// <summary>
    /// Gets or sets the minimum required length for a raw API key.
    /// Defaults to 32.
    /// </summary>
    public int MinLength { get; set; } = 32;

    /// <summary>
    /// Gets or sets a value indicating whether rate limiting is enabled for API keys.
    /// </summary>
    public bool EnableRateLimiting { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number of API key validations allowed per window.
    /// Defaults to 60.
    /// </summary>
    public int RateLimitPerMinute { get; set; } = 60;

    /// <summary>
    /// Gets or sets the window duration for rate limiting in seconds.
    /// Defaults to 60.
    /// </summary>
    public int RateLimitWindowSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the time-to-live for revocation records in the cache.
    /// Defaults to 5 minutes.
    /// </summary>
    public TimeSpan RevocationCacheTtl { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets a value indicating whether to track the last used timestamp for API keys.
    /// Defaults to false.
    /// </summary>
    public bool TrackLastUsedAt { get; set; } = false;
}
