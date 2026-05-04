using System.Security.Claims;
using VK.Blocks.Authorization.Roles.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the Roles authorization feature.
/// </summary>
public sealed record VKRoleOptions : IVKBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAuthorizationBlock.BlockName}:{RolesConstants.FeatureName}";

    /// <summary>
    /// Gets a value indicating whether the roles feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the claim type used to extract the user's role.
    /// </summary>
    public string RoleClaimType { get; init; } = ClaimTypes.Role;
}
