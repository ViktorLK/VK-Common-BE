using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence;

/// <summary>
/// Defines a contract for high-performance bulk operations on database entities.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IVKEntityBulkRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Asynchronously updates entities matching the predicate in a single batch operation.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="setPropertyAction">An action to configure the properties to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The number of rows updated in the database.</returns>
    Task<int> ExecuteUpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Action<IVKPropertySetter<TEntity>> setPropertyAction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously updates entities matching the specification in a single batch operation.
    /// </summary>
    /// <param name="specification">The specification describing entities to update.</param>
    /// <param name="setPropertyAction">An action to configure the properties to update.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The number of rows updated in the database.</returns>
    Task<int> ExecuteUpdateAsync(
        IVKSpecification<TEntity> specification,
        Action<IVKPropertySetter<TEntity>> setPropertyAction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes entities matching the predicate in a single batch operation.
    /// Performs soft delete if the entity implements <see cref="IVKSoftDeletable"/>.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ExecuteDeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes entities matching the specification in a single batch operation.
    /// Performs soft delete if the entity implements <see cref="IVKSoftDeletable"/>.
    /// </summary>
    /// <param name="specification">The specification describing entities to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ExecuteDeleteAsync(
        IVKSpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously permanently (hard) deletes entities matching the predicate, bypassing soft delete.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The number of rows deleted from the database.</returns>
    Task<int> ExecuteHardDeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously permanently (hard) deletes entities matching the specification, bypassing soft delete.
    /// </summary>
    /// <param name="specification">The specification describing entities to hard delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The number of rows deleted from the database.</returns>
    Task<int> ExecuteHardDeleteAsync(
        IVKSpecification<TEntity> specification,
        CancellationToken cancellationToken = default);
}
