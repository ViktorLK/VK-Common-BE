using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Persistence;

/// <summary>
/// Generic repository interface for write-only data operations.
/// </summary>
/// <typeparam name="TEntity">The entity type. Must be a class.</typeparam>
public interface IVKWriteRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Asynchronously adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added entity.</returns>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously adds a range of entities to the repository.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a range of existing entities in the repository.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask UpdateRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity or updates an existing entity.
    /// <para>
    /// Note: In RDBMS (like SQL Server via EF Core), this operation does not guarantee atomic upsert semantics unless implemented with specific database features (e.g., MERGE). It may result in race conditions under high concurrency.
    /// In NoSQL databases (like Cosmos DB), this maps directly to a native atomic Upsert operation.
    /// </para>
    /// </summary>
    /// <param name="entity">The entity to upsert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The upserted entity.</returns>
    Task<TEntity> UpsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity from the repository using the default system policy (soft delete if supported).
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a range of entities from the repository using the default system policy (soft delete if supported).
    /// </summary>
    /// <param name="entities">The entities to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask DeleteRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Forcefully physically deletes an entity from the repository, bypassing soft delete.
    /// </summary>
    /// <param name="entity">The entity to physically delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask HardDeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Forcefully physically deletes a range of entities from the repository, bypassing soft delete.
    /// </summary>
    /// <param name="entities">The entities to physically delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask HardDeleteRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default);
}
