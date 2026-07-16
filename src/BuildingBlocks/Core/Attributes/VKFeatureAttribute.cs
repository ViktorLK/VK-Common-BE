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
    /// Gets the parent BuildingBlock marker type.
    /// </summary>
    public Type Parent { get; } = parent;

    /// <summary>
    /// Gets the logical name of the feature.
    /// </summary>
    public string? Name { get; } = name;

    /// <summary>
    /// Gets or sets the Options type associated with this feature.
    /// </summary>
    public Type? OptionsType { get; set; }

    /// <summary>
    /// Gets or sets the argument generation mode for this feature.
    /// </summary>
    public VKArgsGenerationMode ArgsGenerationMode { get; set; } = VKArgsGenerationMode.None;

    /// <summary>
    /// Gets or sets a value indicating whether this feature is registered by default when registering the block.
    /// </summary>
    public bool RegisterByDefault { get; set; } = true;

    /// <summary>
    /// Gets or sets the custom configuration section name.
    /// </summary>
    public string? SectionName { get; set; }

    /// <summary>
    /// Gets or sets the custom namespace for generated internal components.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Gets or sets the base interface type that the generated Args record should implement (e.g., typeof(IVKAIArgs)).
    /// If specified, all properties of this interface will be generated on the Args record, and the generated record will implement this interface.
    /// </summary>
    public Type? ArgsBaseType { get; set; }
}
