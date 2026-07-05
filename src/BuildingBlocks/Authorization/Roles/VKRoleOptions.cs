using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the Roles authorization feature.
/// </summary>
[VKFeature(typeof(VKAuthorizationBlock), "Roles", GenerateArgs = true)]
public sealed partial record VKRoleOptions : IVKRoleOptions
{
    /// <summary>
    /// Gets a value indicating whether the roles feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the claim type used to extract the user's role.
    /// If null, the global default RoleClaimType is used.
    /// </summary>
    public string? RoleClaimType { get; init; }
}
