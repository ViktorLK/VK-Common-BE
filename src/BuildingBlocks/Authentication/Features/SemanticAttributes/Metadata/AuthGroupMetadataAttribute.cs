using System;
using System.ComponentModel;

namespace VK.Blocks.Authentication.Features.SemanticAttributes.Metadata;

/// <summary>
/// Infrastructure attribute used by source generators to attach authentication group metadata to generated attributes.
/// This allows the <c>AuthenticationMetadataGenerator</c> to discover auth groups from typed attributes.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AuthGroupMetadataAttribute"/> class.
/// </remarks>
/// <param name="groupName">The name of the authentication group (e.g., AuthGroups.User).</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class AuthGroupMetadataAttribute(string groupName) : Attribute
{
    /// <summary>
    /// Gets the name of the authentication group.
    /// </summary>
    public string GroupName { get; } = groupName;
}
