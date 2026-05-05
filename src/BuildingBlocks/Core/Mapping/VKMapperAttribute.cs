using System;

namespace VK.Blocks.Core;

/// <summary>
/// Marks a class or partial method for Source Generated mapping implementation.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class VKMapperAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the source type to map from.
    /// </summary>
    public Type? SourceType { get; init; }

    /// <summary>
    /// Gets or sets the destination type to map to.
    /// </summary>
    public Type? DestinationType { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKMapperAttribute"/> class.
    /// </summary>
    public VKMapperAttribute() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKMapperAttribute"/> class with specific types.
    /// </summary>
    public VKMapperAttribute(Type sourceType, Type destinationType)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
    }
}
