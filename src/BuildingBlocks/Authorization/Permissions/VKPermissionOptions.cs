using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the Permissions authorization feature.
/// </summary>

public sealed partial record VKPermissionOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether the permissions feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the claim type used to extract user permissions.
    /// </summary>
    [VKRequestOverride]
    public string PermissionClaimType { get; init; } = VKAuthorizationClaimTypes.Permissions;

    /// <summary>Request-specific override for Mode.</summary>
    [VKRequestOverride]
    public VKPermissionEvaluationMode? Mode { get; init; }

    /// <summary>Request-specific override for Permissions.</summary>
    [VKRequestOverride]
    public IEnumerable<string>? Permissions { get; init; }

    /// <summary>
    /// Gets a value indicating whether permission evaluation caching is enabled.
    /// </summary>
    public bool EnableCaching { get; init; } = false;

    /// <summary>
    /// Gets the cache expiration time in minutes.
    /// </summary>
    public int CacheExpirationMinutes { get; init; } = 5;

    /// <summary>
    /// Gets the permission inheritance map, where a parent permission grants one or more child permissions.
    /// </summary>
    public Dictionary<string, string[]>? PermissionInheritance { get; init; }
}
