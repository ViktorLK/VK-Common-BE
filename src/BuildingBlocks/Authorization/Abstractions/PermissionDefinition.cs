namespace VK.Blocks.Authorization.Abstractions;

/// <summary>
/// Represents a unique permission with its description.
/// </summary>
public record PermissionDefinition(string Name, string? Description);
