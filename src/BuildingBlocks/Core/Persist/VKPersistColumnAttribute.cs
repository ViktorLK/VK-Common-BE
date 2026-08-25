using System;

namespace VK.Blocks.Core;

/// <summary>
/// Specifies low-level database physical storage details (e.g. TypeName, Precision, Scale).
/// Complements .NET DataAnnotations (such as [StringLength], [Required]).
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class VKPersistColumnAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the database provider-specific column data type (e.g. "jsonb", "nvarchar(max)", "uuid").
    /// </summary>
    public string? TypeName { get; init; }

    /// <summary>
    /// Gets or sets the decimal precision.
    /// </summary>
    public int Precision { get; init; } = -1;

    /// <summary>
    /// Gets or sets the decimal scale.
    /// </summary>
    public int Scale { get; init; } = -1;

    /// <summary>
    /// Gets or sets the database collation.
    /// </summary>
    public string? Collation { get; init; }

    /// <summary>
    /// Gets or sets the explicit column name in the database.
    /// </summary>
    public string? Name { get; init; }
}
