using Microsoft.Azure.Cosmos;

namespace VK.Blocks.Persistence.Cosmos.Provisioning.Internal;

/// <summary>
/// Defines a spatial index to be applied to a path in Cosmos DB container.
/// </summary>
internal sealed record SpatialIndexDefinition
{
    /// <summary>
    /// Gets the path for spatial indexing (e.g., "/location/?").
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets the spatial type (Point, Polygon, LineString, MultiPolygon).
    /// </summary>
    public required SpatialType SpatialType { get; init; }
}
