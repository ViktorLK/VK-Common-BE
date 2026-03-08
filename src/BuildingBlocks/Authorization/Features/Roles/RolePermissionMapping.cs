namespace VK.Blocks.Authorization.Features.Roles;

/// <summary>
/// Represents a mapping between a role and a single permission.
/// </summary>
public sealed record RolePermissionMapping
{
    #region Properties

    /// <summary>
    /// The role name.
    /// </summary>
    public string RoleName { get; init; } = default!;

    /// <summary>
    /// The permission name.
    /// </summary>
    public string PermissionName { get; init; } = default!;

    #endregion
}

