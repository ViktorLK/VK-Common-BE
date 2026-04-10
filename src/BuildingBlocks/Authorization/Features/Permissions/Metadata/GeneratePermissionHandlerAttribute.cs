using System;

namespace VK.Blocks.Authorization.Features.Permissions.Metadata;

/// <summary>
/// Triggers the automatic generation of an <see cref="IPermissionProvider"/> implementation for the decorated class.
/// Must be used in conjunction with <see cref="GeneratePermissionsAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
public sealed class GeneratePermissionHandlerAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the source of permission evaluation. Defaults to <see cref="PermissionSource.Claims"/>.
    /// </summary>
    public PermissionSource Source { get; set; } = PermissionSource.Claims;

    /// <summary>
    /// Gets or sets the module name for the generated class (e.g., "Identity").
    /// If null, it will be inferred from the decorated type's name.
    /// </summary>
    public string? ModuleName { get; set; }
}
