namespace VK.Blocks.Authorization.Features.Permissions.Metadata;

/// <summary>
/// Represents a named permission that can be assigned to roles.
/// </summary>
public sealed record Permission
{
    #region Properties

    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Module { get; init; }

    #endregion
}
