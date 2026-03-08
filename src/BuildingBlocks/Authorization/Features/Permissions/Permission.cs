namespace VK.Blocks.Authorization.Features.Permissions;

/// <summary>
/// Represents a named permission that can be assigned to roles.
/// </summary>
public sealed record Permission
{
    #region Properties

    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public string Module { get; init; } = default!;

    #endregion
}

