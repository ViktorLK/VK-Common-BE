namespace VK.Blocks.Persistence.Cosmos;

/// <summary>
/// Factory interface for creating transaction batch instances.
/// </summary>
public interface IVKCosmosTransactionalBatchFactory
{
    /// <summary>
    /// Creates a partition-scoped transactional batch.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="partitionKey">The partition key targeting the operations.</param>
    IVKCosmosTransactionalBatch<T> CreateBatch<T>(string partitionKey) where T : class;
}
