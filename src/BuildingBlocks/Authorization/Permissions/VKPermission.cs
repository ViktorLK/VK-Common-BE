namespace VK.Blocks.Authorization;

/// <summary>
/// Represents a named VKPermission that can be assigned to roles.
/// </summary>
public sealed record VKPermission
{
    /// <summary>
    /// Gets the unique name of the VKPermission.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the human-readable description of the VKPermission.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the module name this VKPermission belongs to.
    /// </summary>
    public required string Module { get; init; }
}
