using System.Collections.Generic;

namespace VK.Blocks.Persistence.EFCore.Cosmos;

/// <summary>
/// Defines a composite index configuration field.
/// </summary>
public sealed record VKCompositeIndexField
{
    /// <summary>
    /// Gets the path of the property to index.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets a value indicating whether the sorting is descending.
    /// </summary>
    public bool Descending { get; init; } = false;
}

/// <summary>
/// Defines a composite index configuration.
/// </summary>
public sealed record VKCompositeIndexDefinition
{
    /// <summary>
    /// Gets the list of fields forming the composite index.
    /// </summary>
    public required IReadOnlyList<VKCompositeIndexField> Fields { get; init; }
}
