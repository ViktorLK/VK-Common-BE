using VK.Blocks.Authorization.Permissions.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the Permissions authorization feature.
/// </summary>
public sealed record VKPermissionOptions : IVKBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAuthorizationBlock.BlockName}:{PermissionsConstants.FeatureName}";

    /// <summary>
    /// Gets a value indicating whether the permissions feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the claim type used to extract user permissions.
    /// </summary>
    public string PermissionClaimType { get; init; } = VKAuthorizationClaimTypes.Permissions;
}
