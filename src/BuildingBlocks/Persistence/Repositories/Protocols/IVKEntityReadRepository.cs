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
/// Follows AP.01, CS.01, and CS.04 (AsNoTracking by default).
/// </summary>
/// <typeparam name="TEntity">The entity type. Must be a class.</typeparam>
public interface IVKEntityReadRepository<TEntity> where TEntity : class
{
    // =========================================================================
    // 1. Basic Single/Exists & Key-based Lookup
    // =========================================================================

    /// <summary>
    /// Asynchronously finds an entity with the given primary key values (NoTracking by default).
    /// </summary>
    ValueTask<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously finds an entity with the given composite primary key values (NoTracking by default).
    /// </summary>
    ValueTask<TEntity?> GetByIdAsync(object?[]? keyValues, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously determines whether any element satisfies a condition or specification.
    /// </summary>
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously determines whether any element matching the specified specification exists.
    /// </summary>
    Task<bool> AnyAsync(
        IVKSpecification<TEntity> specification,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously returns the number of elements matching a predicate or specification.
    /// </summary>
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously returns the number of elements matching the specified specification.
    /// </summary>
    Task<int> CountAsync(
        IVKSpecification<TEntity> specification,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    // =========================================================================
    // 2. Default Read Queries - NoTracking by default
    // =========================================================================

    /// <summary>
    /// Asynchronously retrieves the first entity matching the specified predicate, or <c>null</c> if no match is found.
    /// </summary>
    Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the first entity matching the specified specification, or <c>null</c> if no match is found.
    /// </summary>
    Task<TEntity?> GetFirstOrDefaultAsync(
        IVKSpecification<TEntity> specification,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the single entity matching the specified predicate.
    /// </summary>
    Task<TEntity?> GetSingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the single entity matching the specified specification.
    /// </summary>
    Task<TEntity?> GetSingleOrDefaultAsync(
        IVKSpecification<TEntity> specification,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a list of entities matching the specified predicate.
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a list of entities matching the specified specification.
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetListAsync(
        IVKSpecification<TEntity> specification,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    // =========================================================================
    // 3. Tracked Read Queries - ChangeTracker Enabled
    // =========================================================================

    /// <summary>
    /// Asynchronously finds an entity with the given primary key values with change tracking enabled.
    /// </summary>
    ValueTask<TEntity?> GetTrackedByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously finds an entity with the given composite primary key values with change tracking enabled.
    /// </summary>
    ValueTask<TEntity?> GetTrackedByIdAsync(object?[]? keyValues, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the first entity matching the specified predicate with change tracking enabled.
    /// </summary>
    Task<TEntity?> GetTrackedFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the first entity matching the specified specification with change tracking enabled.
    /// </summary>
    Task<TEntity?> GetTrackedFirstOrDefaultAsync(
        IVKSpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a single entity matching the specified predicate with change tracking enabled.
    /// Throws if more than one matching entity is found.
    /// </summary>
    Task<TEntity?> GetTrackedSingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a single entity matching the specified specification with change tracking enabled.
    /// Throws if more than one matching entity is found.
    /// </summary>
    Task<TEntity?> GetTrackedSingleOrDefaultAsync(
        IVKSpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a list of entities matching the specified predicate with change tracking enabled.
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetTrackedListAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a list of entities matching the specified specification with change tracking enabled.
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetTrackedListAsync(
        IVKSpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    // =========================================================================
    // 4. Advanced Projection & Stream Queries
    // =========================================================================

    /// <summary>
    /// Evaluates a custom query delegate against the entity set and returns a single projected result or <c>null</c>.
    /// </summary>
    Task<TResult?> QuerySingleAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates a custom query delegate against the entity set and returns a list of projected results.
    /// </summary>
    Task<IReadOnlyList<TResult>> QueryAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously streams entities matching the predicate. Optimized for large datasets.
    /// </summary>
    IAsyncEnumerable<TEntity> StreamAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    // =========================================================================
    // 5. Pagination Queries - Offset & Cursor
    // =========================================================================

    /// <summary>
    /// Asynchronously retrieves a paged list of entities using offset pagination.
    /// </summary>
    Task<VKPagedResult<TEntity>> GetPagedAsync<TKey>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>> orderBy,
        int pageNumber = 1,
        int pageSize = 20,
        bool ascending = true,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a paged list of projected results using offset pagination.
    /// </summary>
    Task<VKPagedResult<TResult>> GetPagedAsync<TKey, TResult>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>> orderBy,
        Expression<Func<TEntity, TResult>> selector,
        int pageNumber = 1,
        int pageSize = 20,
        bool ascending = true,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a paged list of entities matching the specified specification using offset pagination.
    /// </summary>
    Task<VKPagedResult<TEntity>> GetPagedAsync(
        IVKSpecification<TEntity> specification,
        int pageNumber = 1,
        int pageSize = 20,
        bool ascending = true,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a paged list of projected results matching the specified specification using offset pagination.
    /// </summary>
    Task<VKPagedResult<TResult>> GetPagedAsync<TResult>(
        IVKSpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> selector,
        int pageNumber = 1,
        int pageSize = 20,
        bool ascending = true,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a paged list of entities using cursor pagination.
    /// </summary>
    Task<VKCursorPagedResult<TEntity>> GetCursorPagedAsync<TCursor>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TCursor>> cursorSelector,
        TCursor? cursor = default,
        int pageSize = 20,
        bool ascending = true,
        VKCursorDirection direction = VKCursorDirection.Forward,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
        where TCursor : IComparable<TCursor>;

    /// <summary>
    /// Asynchronously retrieves a paged list of projected results using cursor pagination.
    /// </summary>
    Task<VKCursorPagedResult<TResult>> GetCursorPagedAsync<TCursor, TResult>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TCursor>> cursorSelector,
        Expression<Func<TEntity, TResult>> selector,
        TCursor? cursor = default,
        int pageSize = 20,
        bool ascending = true,
        VKCursorDirection direction = VKCursorDirection.Forward,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
        where TCursor : IComparable<TCursor>;

    /// <summary>
    /// Asynchronously retrieves a paged list of entities matching the specified specification using cursor pagination.
    /// </summary>
    Task<VKCursorPagedResult<TEntity>> GetCursorPagedAsync<TCursor>(
        IVKSpecification<TEntity> specification,
        Expression<Func<TEntity, TCursor>> cursorSelector,
        TCursor? cursor = default,
        int pageSize = 20,
        bool ascending = true,
        VKCursorDirection direction = VKCursorDirection.Forward,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
        where TCursor : IComparable<TCursor>;

    /// <summary>
    /// Asynchronously retrieves a paged list of projected results matching the specified specification using cursor pagination.
    /// </summary>
    Task<VKCursorPagedResult<TResult>> GetCursorPagedAsync<TCursor, TResult>(
        IVKSpecification<TEntity> specification,
        Expression<Func<TEntity, TCursor>> cursorSelector,
        Expression<Func<TEntity, TResult>> selector,
        TCursor? cursor = default,
        int pageSize = 20,
        bool ascending = true,
        VKCursorDirection direction = VKCursorDirection.Forward,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
        where TCursor : IComparable<TCursor>;
}

/// <summary>
/// Backward-compatible alias for <see cref="IVKEntityReadRepository{TEntity}"/>.
/// </summary>
