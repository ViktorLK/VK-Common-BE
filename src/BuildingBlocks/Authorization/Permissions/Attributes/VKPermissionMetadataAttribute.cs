using System;
using System.ComponentModel;

namespace VK.Blocks.Authorization;

/// <summary>
/// Infrastructure attribute used by source generators to attach VKPermission metadata to generated attributes.
/// This allows the <c>AuthorizationMetadataGenerator</c> to discover permissions from typed attributes.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="VKPermissionMetadataAttribute"/> class.
/// </remarks>
/// <param name="VKPermission">The VKPermission name.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class VKPermissionMetadataAttribute(string VKPermission) : Attribute
{
    /// <summary>
    /// Gets the VKPermission name.
    /// </summary>
    public string VKPermission { get; } = VKPermission;
}
