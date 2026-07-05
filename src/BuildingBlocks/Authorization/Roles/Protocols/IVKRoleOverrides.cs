namespace VK.Blocks.Authorization;

/// <summary>
/// Defines request-level overrides and target parameters for Role authorization.
/// </summary>
public interface IVKRoleOverrides
{
    /// <summary>
    /// Gets the claim type used to identify the user's role, overriding the default setting.
    /// </summary>
    string? RoleClaimType { get; init; }

    /// <summary>
    /// Gets the array of roles required to pass the authorization check.
    /// </summary>
    string[]? Roles { get; init; }
}
