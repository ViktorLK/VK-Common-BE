using System.Linq.Expressions;
using VK.Blocks.Persistence.Core.Pagination;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Persistence.Abstractions.Repositories;

/// <summary>
/// Generic repository interface for read-only data operations.
/// </summary>
/// <typeparam name="TEntity">The entity type. Must be a class.</typeparam>
public interface IReadRepository<TEntity> where TEntity : class
{
    #region Read (Single)

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

    #endregion

    #region Read (List)

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
    /// <remarks>
    /// <b>⚠�E�EIntentional Escape Hatch  EAdvanced Use Only.</b><br/>
    /// This method is designed for scenarios that cannot be expressed through the standard
    /// repository methods, such as AutoMapper <c>ProjectTo&lt;TDto&gt;()</c> projections
    /// in extension packages (e.g., <c>VK.Blocks.Persistence.EFCore.AutoMapper</c>).<br/><br/>
    /// Unlike exposing <c>IQueryable</c> directly, this method retains:<br/>
    /// • <b>Execution control</b>  Easync execution (<c>ToListAsync</c>) is governed by the repository.<br/>
    /// • <b>Lifecycle safety</b>  E<c>IQueryable</c> is scoped to the delegate; it cannot outlive the <c>DbContext</c>.<br/>
    /// • <b>Observability</b>  Etelemetry, logging, or timing can be added at the repository level.<br/><br/>
    /// For simple filtering or pagination, prefer <see cref="GetListAsync"/> or <see cref="GetPagedAsync{TKey}"/>.
    /// </remarks>
    Task<IReadOnlyList<TResult>> ExecuteAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a custom query delegate against the entity set and returns a single result or <c>null</c>.
    /// </summary>
    /// <remarks>
    /// <b>⚠�E�EIntentional Escape Hatch  EAdvanced Use Only.</b><br/>
    /// Single-result variant of <see cref="ExecuteAsync{TResult}"/>. Applies the same design guarantees:
    /// execution control, lifecycle safety, and observability are all retained by the repository.
    /// Use this for AutoMapper <c>ProjectTo</c> single-entity lookups or complex single-result projections.
    /// </remarks>
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

    #endregion

    #region Read (Paged)

    /// <summary>
    /// Asynchronously retrieves a paged list of entities using offset pagination.
    /// </summary>
    Task<PagedResult<TEntity>> GetPagedAsync<TKey>(
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
    Task<CursorPagedResult<TEntity>> GetCursorPagedAsync<TCursor>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TCursor>> cursorSelector,
        TCursor? cursor = default,
        int pageSize = 20,
        bool ascending = true,
        CursorDirection direction = CursorDirection.Forward,
        CancellationToken cancellationToken = default)
        where TCursor : IComparable<TCursor>;

    #endregion

    #region Aggregates

    /// <summary>
    /// Asynchronously determines whether any element of a sequence satisfies a condition.
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously returns the number of elements in a sequence.
    /// </summary>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    #endregion

    #region Lookups

    /// <summary>
    /// Asynchronously finds an entity with the given primary key values.
    /// </summary>
    ValueTask<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously finds an entity with the given composite primary key values.
    /// </summary>
    ValueTask<TEntity?> GetByIdAsync(object?[]? keyValues, CancellationToken cancellationToken = default);

    #endregion
}
