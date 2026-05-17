using System;

namespace VK.Blocks.Core;

/// <summary>
/// Decoration attribute to trigger automated feature boilerplate generation.
/// This will generate Constants, Marker, and standard DI Registration logic (BB.03).
/// </summary>
/// <param name="parent">The parent BuildingBlock marker type (e.g., typeof(VKAIBlock)).</param>
/// <param name="name">The logical name of the feature (e.g., "Speech", "Agents"). If null, inferred from class name.</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class VKFeatureAttribute(Type parent, string? name = null) : Attribute
{
    /// <summary>
    /// Gets the logical name of the feature.
    /// </summary>
    public string? Name { get; } = name;

    /// <summary>
    /// Gets the parent BuildingBlock marker type.
    /// </summary>
    public Type Parent { get; } = parent;

    /// <summary>
    /// Gets or sets a value indicating whether to generate behavioral boilerplate 
    /// (per-request Args record and the corresponding Options Provider for merging).
    /// Defaults to false.
    /// </summary>
    public bool GenerateArgs { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to generate a standard Options Validator boilerplate.
    /// Defaults to false.
    /// </summary>
    public bool GenerateValidator { get; set; } = false;

    /// <summary>
    /// Gets or sets the custom configuration section name. 
    /// If null, defaults to "{Parent.SectionName}:{Name}".
    /// </summary>
    public string? SectionName { get; set; }

    /// <summary>
    /// Gets or sets the custom namespace for generated internal components.
    /// If null, defaults to "{OptionsNamespace}.Internal".
    /// </summary>
    public string? Namespace { get; set; }
}
