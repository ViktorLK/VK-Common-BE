using Microsoft.Azure.Cosmos;

namespace VK.Blocks.Persistence.EFCore.Cosmos;

/// <summary>
/// Defines partition routing contract for hierarchical partition keys.
/// </summary>
public interface IVKHierarchicalPartitionRoute
{
    /// <summary>
    /// Builds the structured partition key for Cosmos operations.
    /// </summary>
    /// <returns>A built PartitionKey.</returns>
    PartitionKey BuildPartitionKey();
}
