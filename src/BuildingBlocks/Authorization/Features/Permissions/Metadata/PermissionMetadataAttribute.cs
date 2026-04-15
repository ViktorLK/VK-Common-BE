using System;
using System.ComponentModel;

namespace VK.Blocks.Authorization.Features.Permissions.Metadata;

/// <summary>
/// Infrastructure attribute used by source generators to attach permission metadata to generated attributes.
/// This allows the <c>AuthorizationMetadataGenerator</c> to discover permissions from typed attributes.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PermissionMetadataAttribute"/> class.
/// </remarks>
/// <param name="permission">The permission name.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PermissionMetadataAttribute(string permission) : Attribute
{
    /// <summary>
    /// Gets the permission name.
    /// </summary>
    public string Permission { get; } = permission;
}
