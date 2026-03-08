using System.Collections.Generic;

namespace VK.Blocks.Authorization.Features.Roles;

/// <summary>
/// Represents a Role in the system which groups permissions.
/// </summary>
public sealed record Role
{
    #region Properties

    /// <summary>
    /// The unique name of the role.
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// Optional description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Collection of permissions assigned to this role.
    /// </summary>
    public ICollection<string> Permissions { get; init; } = new HashSet<string>();

    #endregion
}

