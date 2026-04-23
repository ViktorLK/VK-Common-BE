using System;

namespace VK.Blocks.Core;

/// <summary>
/// Marks a class as a building block marker.
/// The Source Generator will automatically implement IVKBlockMarker
/// and provide diagnostic fields (Source/Meter) for this class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class VKBlockMarkerAttribute(string? identifier = null) : Attribute
{
    /// <summary>
    /// Gets the unique identifier for this block.
    /// </summary>
    public string? Identifier { get; } = identifier;

    /// <summary>
    /// Gets or sets the version of the block. Defaults to "1.0.0".
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the types of other building blocks this block depends on.
    /// </summary>
    public Type[] Dependencies { get; set; } = [];
}
