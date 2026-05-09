using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence;

/// <summary>
/// Generic repository interface for read-only data operations.
/// </summary>
/// <typeparam name="TEntity">The entity type. Must be a class.</typeparam>
public interface IVKReadRepository<TEntity> where TEntity : class
{

    /// <summary>
    /// Asynchronously retrieves the first entity matching the specified predicate, or <c>null</c> if no match is found.
    /// </summary>
    Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the first entity matching the specified predicate with tracking disabled, or <c>null</c> if no match is found.
    /// </summary>
    Task<TEntity?> GetFirstOrDefaultAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the single entity matching the specified predicate.
    /// Throws an exception if more than one match is found. Returns <c>null</c> if no match is found.
    /// </summary>
    Task<TEntity?> GetSingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the single entity matching the specified predicate with tracking disabled.
    /// Throws an exception if more than one match is found. Returns <c>null</c> if no match is found.
    /// </summary>
    Task<TEntity?> GetSingleOrDefaultAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a list of entities matching the specified predicate.
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a list of entities matching the specified predicate with tracking disabled.
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetListAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a custom query delegate against the entity set and returns a list of results.
    /// </summary>
    Task<IReadOnlyList<TResult>> ExecuteAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a custom query delegate against the entity set and returns a single result or <c>null</c>.
    /// </summary>
    Task<TResult?> ExecuteSingleAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously streams entities matching the predicate.
    /// Optimized for large datasets.
    /// </summary>
    IAsyncEnumerable<TEntity> StreamAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously executes a raw SQL query and maps the results to entities.
    /// </summary>
    Task<IReadOnlyList<TEntity>> FromSqlRawAsync(
        string sql,
        CancellationToken cancellationToken = default,
        params object[] parameters);

    /// <summary>
    /// Asynchronously retrieves a paged list of entities using offset pagination.
    /// </summary>
    Task<VKPagedResult<TEntity>> GetPagedAsync<TKey>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>> orderBy,
        int pageNumber = 1,
        int pageSize = 20,
        bool ascending = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a paged list of entities using cursor pagination.
    /// The cursor property must be indexed and unique.
    /// </summary>
    Task<VKCursorPagedResult<TEntity>> GetCursorPagedAsync<TCursor>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TCursor>> cursorSelector,
        TCursor? cursor = default,
        int pageSize = 20,
        bool ascending = true,
        VKCursorDirection direction = VKCursorDirection.Forward,
        CancellationToken cancellationToken = default)
        where TCursor : IComparable<TCursor>;

    /// <summary>
    /// Asynchronously determines whether any element of a sequence satisfies a condition.
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously returns the number of elements in a sequence.
    /// </summary>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously finds an entity with the given primary key values.
    /// </summary>
    ValueTask<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously finds an entity with the given composite primary key values.
    /// </summary>
    ValueTask<TEntity?> GetByIdAsync(object?[]? keyValues, CancellationToken cancellationToken = default);

}
