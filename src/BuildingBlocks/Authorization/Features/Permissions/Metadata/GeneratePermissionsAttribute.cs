using System;

namespace VK.Blocks.Authorization.Features.Permissions.Metadata;

/// <summary>
/// Marks a class or struct as a container for permission definitions to be
/// automatically cataloged and processed by the source generator.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class GeneratePermissionsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the logical module name for all permissions defined in this container.
    /// </summary>
    public string? Module { get; set; }
}
