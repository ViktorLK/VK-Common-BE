using Microsoft.AspNetCore.Authentication.Cookies;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Configuration options for Cookie-based session authentication.
/// </summary>
public sealed partial record VKCookieOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Cookie-based authentication is enabled.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets or sets the authentication scheme name for Cookie auth.
    /// Default is "Cookies".
    /// </summary>
    public string SchemeName { get; init; } = CookieAuthenticationDefaults.AuthenticationScheme;

    /// <summary>
    /// Gets or sets the name of the session cookie.
    /// </summary>
    public string CookieName { get; init; } = "vk.sid";

    /// <summary>
    /// Gets or sets the lifetime expiration in minutes for the session.
    /// </summary>
    public int ExpireMinutes { get; init; } = 120;

    /// <summary>
    /// Gets or sets a value indicating whether sliding expiration is active.
    /// </summary>
    public bool SlidingExpiration { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum number of concurrent active sessions allowed per user.
    /// 0 means unlimited.
    /// </summary>
    public int MaxConcurrentSessions { get; init; } = 0;

    /// <summary>
    /// Gets or sets a value indicating whether to revoke the oldest session (true)
    /// or reject the login request (false) when maximum concurrent sessions is exceeded.
    /// </summary>
    public bool KickOldestSession { get; init; } = true;
}
