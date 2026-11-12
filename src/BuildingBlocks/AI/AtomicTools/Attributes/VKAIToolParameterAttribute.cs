using System;

namespace VK.Blocks.AI;

/// <summary>
/// Specifies metadata for a parameter of a compile-time generated AI atomic tool.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class VKAIToolParameterAttribute : Attribute
{
    /// <summary>
    /// Gets the description of the parameter.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets or sets whether the parameter is required.
    /// If not explicitly set, determined by type nullability or optionality.
    /// </summary>
    public bool Required { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKAIToolParameterAttribute"/> class.
    /// </summary>
    /// <param name="description">The description of what this parameter represents.</param>
    public VKAIToolParameterAttribute(string description)
    {
        Description = description;
    }
}

