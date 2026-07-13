using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.Cosmos;

/// <summary>
/// Abstraction for Cosmos DB Transactional Batch operations.
/// Operations must target the same logical partition key. Maximum operations: 100. Total size: 2MB.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IVKCosmosTransactionalBatch<T> where T : class
{
    /// <summary>
    /// Enqueues a create operation.
    /// </summary>
    IVKCosmosTransactionalBatch<T> Create(T entity);

    /// <summary>
    /// Enqueues an upsert operation.
    /// </summary>
    IVKCosmosTransactionalBatch<T> Upsert(T entity);

    /// <summary>
    /// Enqueues a delete operation by ID.
    /// </summary>
    IVKCosmosTransactionalBatch<T> Delete(string id);

    /// <summary>
    /// Executes all enqueued operations atomically.
    /// </summary>
    Task<VKResult> ExecuteAsync(CancellationToken cancellationToken);
}
