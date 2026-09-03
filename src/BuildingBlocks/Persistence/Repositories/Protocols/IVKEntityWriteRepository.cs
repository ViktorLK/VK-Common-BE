using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Persistence;

/// <summary>
/// Generic repository interface for write-only data operations on database entities.
/// </summary>
/// <typeparam name="TEntity">The entity type. Must be a class.</typeparam>
public interface IVKEntityWriteRepository<TEntity> where TEntity : class
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
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a range of entities in the repository.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask UpdateRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts an entity into the database (updates if exists, adds otherwise).
    /// </summary>
    /// <param name="entity">The entity to upsert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added or updated entity.</returns>
    Task<TEntity> UpsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity from the repository (performs soft delete if supported).
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a range of entities from the repository (performs soft delete if supported).
    /// </summary>
    /// <param name="entities">The entities to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask DeleteRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a permanent (hard) delete, ignoring any soft delete mechanisms.
    /// </summary>
    /// <param name="entity">The entity to permanently delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask HardDeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a permanent (hard) delete on a range of entities, ignoring soft delete mechanisms.
    /// </summary>
    /// <param name="entities">The entities to permanently delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask HardDeleteRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default);
}
