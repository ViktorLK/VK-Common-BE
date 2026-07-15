namespace VK.Blocks.Persistence.EFCore.Cosmos;

/// <summary>
/// Defines consistency levels for Cosmos DB operations.
/// </summary>
public enum VKCosmosConsistencyLevel
{
    /// <summary>
    /// Strong consistency.
    /// </summary>
    Strong,

    /// <summary>
    /// Bounded Staleness consistency.
    /// </summary>
    BoundedStaleness,

    /// <summary>
    /// Session consistency.
    /// </summary>
    Session,

    /// <summary>
    /// Consistent Prefix consistency.
    /// </summary>
    ConsistentPrefix,

    /// <summary>
    /// Eventual consistency.
    /// </summary>
    Eventual
}
