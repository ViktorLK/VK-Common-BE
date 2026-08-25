using System;

namespace VK.Blocks.Core;

/// <summary>
/// Specifies that a property should be ignored by EF Core persistence mapping and Source Generators.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class VKPersistIgnoreAttribute : Attribute
{
}
