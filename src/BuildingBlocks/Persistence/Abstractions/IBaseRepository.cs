using System.Linq.Expressions;
using VK.Blocks.Persistence.Abstractions.Pagination;

namespace VK.Blocks.Persistence.Abstractions;

/// <summary>
/// Generic repository interface for data persistence.
/// </summary>
/// <typeparam name="TEntity">The entity type. Must be a class.</typeparam>
public interface IBaseRepository<TEntity> where TEntity : class
{
    #region Read (Single)

    /// <summary>
    /// Asynchronously retrieves the first entity matching the specified predicate.
    /// Returns null if no match is found.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <param name="include">Optional navigation properties to include.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first matching entity, or null.</returns>
    Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the first entity matching the specified predicate with tracking disabled.
    /// Returns null if no match is found.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <param name="include">Optional navigation properties to include.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first matching entity, or null.</returns>
    Task<TEntity?> GetFirstOrDefaultAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the single entity matching the specified predicate.
    /// Throws an exception if more than one match is found. Returns null if no match is found.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <param name="include">Optional navigation properties to include.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The single matching entity, or null.</returns>
    Task<TEntity?> GetSingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the single entity matching the specified predicate with tracking disabled.
    /// Throws an exception if more than one match is found. Returns null if no match is found.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <param name="include">Optional navigation properties to include.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The single matching entity, or null.</returns>
    Task<TEntity?> GetSingleOrDefaultAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Read (List)

    /// <summary>
    /// Asynchronously retrieves a list of entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <param name="include">Optional navigation properties to include.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of matching entities.</returns>
    Task<IReadOnlyList<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a list of entities matching the specified predicate with tracking disabled.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <param name="include">Optional navigation properties to include.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of matching entities.</returns>
    Task<IReadOnlyList<TEntity>> GetListAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously executes a query built by the provided function.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="builder">The query builder function.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of query results.</returns>
    Task<IReadOnlyList<TResult>> ExecuteAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously executes a query built by the provided function and returns a single result.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="builder">The query builder function.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The single query result, or null.</returns>
    Task<TResult?> ExecuteSingleAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously streams entities matching the predicate.
    /// Optimized for large datasets.
    /// </summary>
    /// <param name="predicate">Optional filter predicate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An asynchronous stream of entities.</returns>
    IAsyncEnumerable<TEntity> StreamAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously executes a raw SQL query and maps the results to entities.
    /// </summary>
    /// <param name="sql">The raw SQL query string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="parameters">SQL parameters.</param>
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
    /// <param name="predicate">Optional filter predicate.</param>
    /// <param name="orderBy">Sorting expression.</param>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="ascending">Sort direction (true for ascending).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the items and the total count.</returns>
    Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPagedAsync<TKey>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>> orderBy,
        int pageNumber,
        int pageSize,
        bool ascending = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a paged list of entities using cursor pagination.
    /// The cursor property must be indexed and unique.
    /// </summary>
    /// <typeparam name="TKey">The type of the cursor property.</typeparam>
    /// <param name="predicate">Optional filter predicate.</param>
    /// <param name="cursorProperty">The property to use as the cursor.</param>
    /// <param name="lastValue">The cursor value from the last item of the previous page.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="ascending">Sort direction (true for ascending).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the items and the next cursor.</returns>
    Task<CursorPagedResult<TEntity>> GetCursorPagedAsync<TKey>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>> cursorProperty,
        TKey? lastValue,
        int pageSize,
        bool ascending = true,
        CancellationToken cancellationToken = default);

    #endregion

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
    void Update(TEntity entity);

    /// <summary>
    /// Updates a range of entities.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    void UpdateRange(IReadOnlyList<TEntity> entities);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    void Delete(TEntity entity);

    /// <summary>
    /// Asynchronously deletes entities matching the predicate.
    /// Supports soft delete if the entity implements ISoftDelete.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of affected rows.</returns>
    Task<int> ExecuteDeleteRangeAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously updates entities matching the predicate using the provided property setters.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <param name="propertySetter">The property setters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of affected rows.</returns>
    Task<int> ExecuteUpdateRangeAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IPropertySetter<TEntity>, IPropertySetter<TEntity>> propertySetter,
        CancellationToken cancellationToken = default);

    #endregion

    #region Aggregates

    /// <summary>
    /// Asynchronously determines whether any element of a sequence satisfies a condition.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the source sequence contains any elements in the source sequence; otherwise, false.</returns>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously returns the number of elements in a sequence.
    /// </summary>
    /// <param name="predicate">Optional filter predicate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity found, or null.</returns>
    ValueTask<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously finds an entity with the given primary key values.
    /// If an entity with the given primary key values is being tracked by the context, then it is returned immediately without making a request to the database.
    /// Otherwise, a query is made to the database for an entity with the given primary key values and this entity, if found, is attached to the context and returned.
    /// </summary>
    /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity found, or null.</returns>
    ValueTask<TEntity?> GetByIdAsync(object?[]? keyValues, CancellationToken cancellationToken = default);

    #endregion
}
