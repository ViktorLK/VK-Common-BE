using System;

namespace VK.Blocks.Core;

/// <summary>
/// Marks an entity property to be serialized and persisted as a structured JSON payload.
/// The Source Generator automatically emits compile-time ValueConverters and ValueComparers.
/// Follows [AP.01], [CS.08].
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class VKPersistJsonAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the maximum string length when persisted to relational text columns.
    /// Default is 4000 (compliant with CS.08 bounded string rule).
    /// </summary>
    public int MaxLength { get; init; } = 4000;
}
