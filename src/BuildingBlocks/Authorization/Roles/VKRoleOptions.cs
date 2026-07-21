using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the Roles authorization feature.
/// </summary>

public sealed partial record VKRoleOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether the roles feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the claim type used to extract the user's role.
    /// If null, the global default RoleClaimType is used.
    /// </summary>
    [VKRequestOverride]
    public string? RoleClaimType { get; init; }

    /// <summary>Request-specific override for Roles.</summary>
    [VKRequestOverride]
    public string[]? Roles { get; init; }

    /// <summary>
    /// Gets a value indicating whether role evaluation caching is enabled.
    /// </summary>
    public bool EnableCaching { get; init; } = false;

    /// <summary>
    /// Gets the cache expiration time in minutes.
    /// </summary>
    public int CacheExpirationMinutes { get; init; } = 5;

    /// <summary>
    /// Gets the role inheritance map, where the key is the parent role, and the value is the array of child roles it inherits.
    /// </summary>
    public Dictionary<string, string[]>? RoleInheritance { get; init; }
}
