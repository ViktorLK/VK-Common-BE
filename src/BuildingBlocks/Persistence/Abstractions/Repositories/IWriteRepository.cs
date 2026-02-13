using System.Linq.Expressions;

namespace VK.Blocks.Persistence.Abstractions.Repositories;

/// <summary>
/// Generic repository interface for write-only data operations.
/// </summary>
/// <typeparam name="TEntity">The entity type. Must be a class.</typeparam>
public interface IWriteRepository<TEntity> where TEntity : class
{
    #region Write

    /// <summary>
    /// Asynchronously adds a new entity.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added entity.</returns>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously adds a range of entities.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    ValueTask UpdateAsync(TEntity entity);

    /// <summary>
    /// Updates a range of entities.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    ValueTask UpdateRangeAsync(IReadOnlyList<TEntity> entities);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    ValueTask DeleteAsync(TEntity entity);

    /// <summary>
    /// Deletes a range of entities.
    /// </summary>
    /// <param name="entities">The entities to delete.</param>
    ValueTask DeleteRangeAsync(IReadOnlyList<TEntity> entities);

    /// <summary>
    /// Executes a bulk update operation on entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The condition to filter entities to be updated.</param>
    /// <param name="setPropertyAction">The action defining the properties to update.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ExecuteUpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Action<IPropertySetter<TEntity>> setPropertyAction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a bulk delete operation on entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The condition to filter entities to be deleted.</param>
    /// <param name="forceDelete">If true, the delete operation will be forced even if the entity implements ISoftDelete.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ExecuteDeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool forceDelete = false,
        CancellationToken cancellationToken = default);

    #endregion
}
