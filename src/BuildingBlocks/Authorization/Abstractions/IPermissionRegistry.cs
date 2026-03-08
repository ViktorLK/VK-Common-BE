using System.Collections.Generic;

namespace VK.Blocks.Authorization.Abstractions;

/// <summary>
/// Defines a registry for managing permission definitions.
/// </summary>
public interface IPermissionRegistry
{
    /// <summary>
    /// Registers a new permission with an optional description.
    /// </summary>
    void Register(string permission, string? description = null);

    /// <summary>
    /// Retrieves all registered permissions.
    /// </summary>
    IEnumerable<PermissionDefinition> GetAll();
}

