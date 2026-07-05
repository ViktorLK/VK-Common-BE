using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Defines global static options for Permissions authorization.
/// </summary>
public interface IVKPermissionOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets the claim type used to extract user permissions.
    /// </summary>
    string PermissionClaimType { get; init; }
}
