using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the authorization building block.
/// </summary>
public sealed record VKAuthorizationOptions : IVKBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAuthorizationBlock.BlockName}";

    /// <summary>
    /// Gets a value indicating whether authorization is enabled globally.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the claim type used to extract roles (for SuperAdmin check).
    /// </summary>
    public string RoleClaimType { get; init; } = VKAuthorizationClaimTypes.Role;

    /// <summary>
    /// Gets the role name that can bypass all authorization checks (SuperAdmin).
    /// If null or empty, bypass is disabled.
    /// </summary>
    public string? SuperAdminRole { get; init; } = VKBlocksConstants.SuperAdminRole;
}
