using System;

namespace VK.Blocks.Core;

/// <summary>
/// Marks a class as a feature marker belonging to a parent building block.
/// The Source Generator will automatically implement IVKFeatureMarker
/// and proxy diagnostic fields to the parent block.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class VKFeatureMarkerAttribute(string identifier, Type parentBlockType) : Attribute
{
    /// <summary>
    /// Gets the unique short identifier for this feature (e.g., "Permissions").
    /// The SG will combine this with the parent's identifier.
    /// </summary>
    public string Identifier { get; } = identifier;

    /// <summary>
    /// Gets the type of the parent building block (must implement IVKBlockMarker).
    /// </summary>
    public Type ParentBlockType { get; } = parentBlockType;

    /// <summary>
    /// Gets or sets the version of the feature. 
    /// If null, the SG will inherit the version from the parent block.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this feature is optional. Defaults to true.
    /// </summary>
    public bool IsOptional { get; set; } = true;
}
