using System;

namespace VK.Blocks.Core;

/// <summary>
/// Configures database index and triggers automatic generation of:
/// 1. EF Core Index with standardized industrial name (IX_{Table}_{Columns} / UX_{Table}_{Columns}).
/// 2. Strongly-typed Query Object extensions (GetByXxxAsync, ExistsByXxxAsync, CountByXxxAsync).
/// 3. Reusable Specifications (ByXxx).
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public sealed class VKPersistIndexAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a value indicating whether the index is unique.
    /// </summary>
    public bool IsUnique { get; init; }

    /// <summary>
    /// Gets or sets the composite index group name. Properties sharing the same group name form a composite index.
    /// </summary>
    public string? Group { get; init; }

    /// <summary>
    /// Gets or sets the column order within a composite index group.
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// Gets or sets an explicit database index name override.
    /// If omitted, standard industrial convention (IX/UX_{Table}_{Columns}) is used.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate query methods and specifications for this index.
    /// Defaults to true.
    /// </summary>
    public bool GenerateQuery { get; init; } = true;
}
