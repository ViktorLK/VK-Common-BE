namespace VK.Blocks.Persistence.Cosmos;

/// <summary>
/// Entities can implement this interface to define their partition routing deterministically,
/// preventing hot partition issues.
/// </summary>
public interface IVKPartitionRoute
{
    /// <summary>
    /// Computes the logical partition key for the entity.
    /// </summary>
    /// <returns>The computed partition key string.</returns>
    string GetPartitionKey();
}
