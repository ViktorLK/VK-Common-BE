using System;

namespace VK.Blocks.Core;

/// <summary>
/// Decoration attribute to trigger automated defaults feature generation for a block.
/// </summary>
/// <param name="parent">The parent BuildingBlock marker type (e.g., typeof(VKAuthorizationBlock)).</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class VKDefaultsAttribute(Type parent) : Attribute
{
    /// <summary>
    /// Gets the parent BuildingBlock marker type.
    /// </summary>
    public Type Parent { get; } = parent;
}
