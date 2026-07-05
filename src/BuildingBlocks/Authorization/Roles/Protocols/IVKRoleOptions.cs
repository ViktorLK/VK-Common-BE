using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Defines global static options for Role authorization.
/// </summary>
public interface IVKRoleOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets the claim type used to identify the user's role.
    /// </summary>
    string? RoleClaimType { get; init; }
}
