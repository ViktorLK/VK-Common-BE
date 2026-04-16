namespace VK.Blocks.Authorization.Features.Permissions.Metadata;

/// <summary>
/// Represents a named permission that can be assigned to roles.
/// </summary>
public sealed record Permission
{
    /// <summary>
    /// Gets the unique name of the permission.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the human-readable description of the permission.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the module name this permission belongs to.
    /// </summary>
    public required string Module { get; init; }
}
