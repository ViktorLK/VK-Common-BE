using System;

namespace VK.Blocks.Core;

/// <summary>
/// Specifies explicit primary key configuration (support composite keys via <see cref="Order"/>).
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class VKPersistKeyAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the column order for composite primary keys.
    /// </summary>
    public int Order { get; init; }
}
