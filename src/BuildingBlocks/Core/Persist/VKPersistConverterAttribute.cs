using System;

namespace VK.Blocks.Core;

/// <summary>
/// Specifies an EF Core ValueConverter type for the property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class VKPersistConverterAttribute(Type converterType) : Attribute
{
    /// <summary>
    /// Gets the ValueConverter type (must inherit from ValueConverter or have a parameterless constructor).
    /// </summary>
    public Type ConverterType => converterType;
}
