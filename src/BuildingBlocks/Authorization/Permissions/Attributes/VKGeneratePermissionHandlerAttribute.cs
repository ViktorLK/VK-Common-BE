using System;

namespace VK.Blocks.Authorization;

/// <summary>
/// Triggers the automatic generation of an <see cref="IVKPermissionProvider"/> implementation for the decorated class.
/// Must be used in conjunction with <see cref="VKGeneratePermissionsAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
public sealed class VKGeneratePermissionHandlerAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the source of VKPermission evaluation. Defaults to <see cref="VKPermissionSource.Claims"/>.
    /// </summary>
    public VKPermissionSource Source { get; set; } = VKPermissionSource.Claims;

    /// <summary>
    /// Gets or sets the module name for the generated class (e.g., "Identity").
    /// If null, it will be inferred from the decorated type's name.
    /// </summary>
    public string? ModuleName { get; set; }
}
