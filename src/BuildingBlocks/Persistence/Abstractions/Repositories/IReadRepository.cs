using System.Linq.Expressions;
using VK.Blocks.Persistence.Abstractions.Pagination;

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
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="include">A function to include navigation properties.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The first element that matches the conditions, or <c>null</c> if no matching element is found.</returns>
    Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the first entity matching the specified predicate with tracking disabled, or <c>null</c> if no match is found.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="include">A function to include navigation properties.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The first element that matches the conditions, or <c>null</c> if no matching element is found.</returns>
    Task<TEntity?> GetFirstOrDefaultAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the single entity matching the specified predicate.
    /// Throws an exception if more than one match is found. Returns <c>null</c> if no match is found.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="include">A function to include navigation properties.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The single element that matches the conditions, or <c>null</c> if no matching element is found.</returns>
    Task<TEntity?> GetSingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the single entity matching the specified predicate with tracking disabled.
    /// Throws an exception if more than one match is found. Returns <c>null</c> if no match is found.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="include">A function to include navigation properties.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The single element that matches the conditions, or <c>null</c> if no matching element is found.</returns>
    Task<TEntity?> GetSingleOrDefaultAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Read (List)

    /// <summary>
    /// Asynchronously retrieves a list of entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="include">A function to include navigation properties.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of elements that match the conditions.</returns>
    Task<IReadOnlyList<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a list of entities matching the specified predicate with tracking disabled.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="include">A function to include navigation properties.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of elements that match the conditions.</returns>
    Task<IReadOnlyList<TEntity>> GetListAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously executes a query built by the provided function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="builder">A function to build the query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of query results.</returns>
    Task<IReadOnlyList<TResult>> ExecuteAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously executes a query built by the provided function and returns a single result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="builder">A function to build the query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The single query result, or <c>null</c>.</returns>
    Task<TResult?> ExecuteSingleAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously streams entities matching the predicate.
    /// Optimized for large datasets.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous stream of entities.</returns>
    IAsyncEnumerable<TEntity> StreamAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously executes a raw SQL query and maps the results to entities.
    /// </summary>
    /// <param name="sql">The raw SQL query string.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="parameters">The SQL parameters.</param>
    /// <returns>A list of entities returned by the query.</returns>
    Task<IReadOnlyList<TEntity>> FromSqlRawAsync(
        string sql,
        CancellationToken cancellationToken = default,
        params object[] parameters);

    #endregion

    #region Read (Paged)

    /// <summary>
    /// Asynchronously retrieves a paged list of entities using offset pagination.
    /// </summary>
    /// <typeparam name="TKey">The type of the sorting key.</typeparam>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="ascending">Sort direction (<c>true</c> for ascending).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A tuple containing the items and the total count.</returns>
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
    /// <typeparam name="TCursor">The type of the cursor property.</typeparam>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cursorSelector">The property to use as the cursor.</param>
    /// <param name="cursor">The cursor value from the last item of the previous page.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="ascending">Sort direction (<c>true</c> for ascending).</param>
    /// <param name="direction">The direction of the cursor pagination.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the items and the next cursor.</returns>
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
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if any elements in the source sequence pass the test in the specified predicate; otherwise, <c>false</c>.</returns>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously returns the number of elements in a sequence.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The number of elements in the input sequence.</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    #endregion

    #region Lookups

    /// <summary>
    /// Asynchronously finds an entity with the given primary key values.
    /// If an entity with the given primary key values is being tracked by the context, then it is returned immediately without making a request to the database.
    /// Otherwise, a query is made to the database for an entity with the given primary key values and this entity, if found, is attached to the context and returned.
    /// </summary>
    /// <param name="id">The value of the primary key for the entity to be found.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The entity found, or <c>null</c>.</returns>
    ValueTask<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously finds an entity with the given primary key values.
    /// If an entity with the given primary key values is being tracked by the context, then it is returned immediately without making a request to the database.
    /// Otherwise, a query is made to the database for an entity with the given primary key values and this entity, if found, is attached to the context and returned.
    /// </summary>
    /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The entity found, or <c>null</c>.</returns>
    ValueTask<TEntity?> GetByIdAsync(object?[]? keyValues, CancellationToken cancellationToken = default);

    #endregion
}
