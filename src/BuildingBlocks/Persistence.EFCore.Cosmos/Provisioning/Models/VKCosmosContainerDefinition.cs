using System.Collections.Generic;

namespace VK.Blocks.Persistence.Cosmos;

/// <summary>
/// Configuration properties for provisioning a Cosmos DB Container.
/// </summary>
public sealed record VKCosmosContainerDefinition
{
    /// <summary>
    /// Gets the name of the container.
    /// </summary>
    public required string ContainerName { get; init; }

    /// <summary>
    /// Gets the primary partition key path (e.g., "/partitionKey").
    /// </summary>
    public required string PartitionKeyPath { get; init; }

    /// <summary>
    /// Gets hierarchical partition key paths if using hierarchical partitioning.
    /// If specified, these override PartitionKeyPath.
    /// </summary>
    public IReadOnlyList<string>? HierarchicalPartitionKeyPaths { get; init; }

    /// <summary>
    /// Gets the default Time to Live (TTL) in seconds.
    /// If null, TTL is disabled. If -1, items never expire by default.
    /// </summary>
    public int? DefaultTimeToLiveSeconds { get; init; }

    /// <summary>
    /// Gets a value indicating whether Analytical Store is enabled on this container.
    /// </summary>
    public bool EnableAnalyticalStore { get; init; } = false;
}
