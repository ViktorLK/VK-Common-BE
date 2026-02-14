using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VK.Blocks.Common;
using VK.Blocks.Persistence.Abstractions.Auditing;
using VK.Blocks.Persistence.Abstractions.Entities;
using VK.Blocks.Persistence.Abstractions.Pagination;
using VK.Blocks.Persistence.Abstractions.Repositories;
using VK.Blocks.Persistence.EFCore.Caches;
using VK.Blocks.Persistence.EFCore.Extensions;
using VK.Blocks.Persistence.EFCore.Services;

namespace VK.Blocks.Persistence.EFCore.Repositories;

/// <summary>
/// Implementation of the generic repository base class.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="EfCoreRepository{TEntity}"/> class.
/// </remarks>
/// <param name="context">The database context.</param>
/// <param name="logger">The logger.</param>
/// <param name="processor">The entity lifecycle processor.</param>
public class EfCoreRepository<TEntity>(
    DbContext context,
    ILogger<EfCoreRepository<TEntity>> logger,
    IEntityLifecycleProcessor? processor = null) : IBaseRepository<TEntity>
    where TEntity : class
{
    #region Fields

    /// <summary>
    /// The database context.
    /// </summary>
    protected readonly DbContext Context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// The DB set for the entity.
    /// </summary>
    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

    /// <summary>
    /// The logger instance.
    /// </summary>
    protected readonly ILogger<EfCoreRepository<TEntity>> _logger = logger;

    /// <summary>
    /// The entity lifecycle processor.
    /// </summary>
    protected readonly IEntityLifecycleProcessor? _processor = processor;

    #endregion
    #region Constructors

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
            => GetEntityInternalAsync(predicate, false, include, true, cancellationToken);

    /// <inheritdoc />
    public Task<TEntity?> GetFirstOrDefaultAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
            => GetEntityInternalAsync(predicate, true, include, true, cancellationToken);

    /// <inheritdoc />
    public Task<TEntity?> GetSingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
            => GetEntityInternalAsync(predicate, false, include, false, cancellationToken);

    /// <inheritdoc />
    public Task<TEntity?> GetSingleOrDefaultAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
            => GetEntityInternalAsync(predicate, true, include, false, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
            => GetListInternalAsync(predicate, false, include, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<TEntity>> GetListAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
            => GetListInternalAsync(predicate, true, include, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<TResult>> ExecuteAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        CancellationToken cancellationToken = default)
    {
        return await builder(GetQueryable(true)).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TResult?> ExecuteSingleAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        CancellationToken cancellationToken = default)
    {
        return await builder(GetQueryable(true)).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<TEntity> StreamAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(true).WhereIf(predicate is not null, predicate!);

        await foreach (var entity in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return entity;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> FromSqlRawAsync(
        string sql,
        CancellationToken cancellationToken = default,
        params object[] parameters)
    {
        return await DbSet
            .FromSqlRaw(sql, parameters)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<PagedResult<TEntity>> GetPagedAsync<TKey>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>> orderBy,
        int pageNumber = 1,
        int pageSize = 20,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        PaginationValidator.ValidateOffsetPagination(pageNumber, pageSize);

        var query = GetQueryable(true).WhereIf(predicate is not null, predicate!);
        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        if (totalCount == 0)
        {
            return new PagedResult<TEntity>
            {
                Items = Array.Empty<TEntity>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0
            };
        }

        var offset = (pageNumber - 1) * pageSize;
        var items = await query
            .OrderByIf(ascending, orderBy)
            .Skip(offset)
            .Take(pageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PagedResult<TEntity>
        {
            Items = items.AsReadOnly(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc />
    public async Task<CursorPagedResult<TEntity>> GetCursorPagedAsync<TCursor>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TCursor>> cursorSelector,
        TCursor? cursor = default,
        int pageSize = 20,
        bool ascending = true,
        CursorDirection direction = CursorDirection.Forward,
        CancellationToken cancellationToken = default)
        where TCursor : IComparable<TCursor>
    {
        ArgumentNullException.ThrowIfNull(cursorSelector);

        if (pageSize <= 0)
        {
            throw new ArgumentException("pageSize must greater than 0", nameof(pageSize));
        }

        var hasCursor = !EqualityComparer<TCursor>.Default.Equals(cursor, default);

        // Rationale: We fetch one extra item to determine if there is a next page.
        // If cursor is present, we filter by cursor position using BuildCursorExpression.
        var items = await GetQueryable(true)
            .WhereIf(predicate is not null, predicate!)
            .WhereIf(cursor is not null && hasCursor, BuildCursorExpression(cursorSelector, cursor!, ascending, direction))
            .OrderByCursorDirection(ascending, cursorSelector!, direction)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var hasMore = items.Count > pageSize;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

        if (direction == CursorDirection.Backward)
        {
            items.Reverse();
        }

        var compiledSelector = EfCoreExpressionCache<TEntity, TCursor>.GetOrCompile(cursorSelector!);

        var nextCursor = hasMore && direction == CursorDirection.Forward && items.Count != 0 ? compiledSelector(items[^1]) : default;

        var previousCursor = hasMore && direction == CursorDirection.Backward && items.Count != 0 ? compiledSelector(items[0]) : default;

        var nextCursorString = nextCursor != null && !EqualityComparer<TCursor>.Default.Equals(nextCursor, default)
            ? EncodeCursor(nextCursor)
            : null;

        var previousCursorString = previousCursor != null && !EqualityComparer<TCursor>.Default.Equals(previousCursor, default)
            ? EncodeCursor(previousCursor)
            : null;

        return new CursorPagedResult<TEntity>
        {
            Items = items.AsReadOnly(),
            NextCursor = nextCursorString,
            PreviousCursor = previousCursorString,
            HasNextPage = direction == CursorDirection.Forward && hasMore,
            HasPreviousPage = direction == CursorDirection.Backward && hasMore,
            PageSize = pageSize
        };
    }

    /// <inheritdoc />
    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entry = await DbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        return entry.Entity;
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        await DbSet.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public ValueTask UpdateAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Update(entity);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask UpdateRangeAsync(IReadOnlyList<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        DbSet.UpdateRange(entities);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<int> ExecuteUpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Action<IPropertySetter<TEntity>> setPropertyAction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(setPropertyAction);

        var propertySetter = new EfCorePropertySetter<TEntity>();
        setPropertyAction(propertySetter);

        if (_processor is not null)
        {
            _processor.ProcessBulkUpdate(propertySetter);
        }

        var setPropertyExpression = propertySetter.BuildSetPropertyExpression();
        var updatedRows = await DbSet.Where(predicate).ExecuteUpdateAsync(setPropertyExpression, cancellationToken);

        _logger.LogInformation("Bulk update affected {Count} rows for {EntityType}", updatedRows, typeof(TEntity).Name);

        return updatedRows;
    }

    /// <inheritdoc />
    public async Task<int> ExecuteDeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool forceDelete = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var query = GetQueryable(false).Where(predicate);

        if (!forceDelete && _processor is not null && EfCoreTypeCache<TEntity>.IsSoftDelete)
        {
            var propertySetter = new EfCorePropertySetter<TEntity>();

            _processor.ProcessBulkSoftDelete(propertySetter);

            var setPropertyExpression = propertySetter.BuildSetPropertyExpression();
            var softDeletedRows = await query.ExecuteUpdateAsync(setPropertyExpression, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Bulk softdelete affected {Count} rows for {EntityType}", softDeletedRows, typeof(TEntity).Name);

            return softDeletedRows;
        }

        var deletedRows = await query.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Bulk delete affected {Count} rows for {EntityType}", deletedRows, typeof(TEntity).Name);

        return deletedRows;
    }

    /// <inheritdoc />
    public ValueTask DeleteAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Remove(entity);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DeleteRangeAsync(IReadOnlyList<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        DbSet.RemoveRange(entities);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(true)
            .WhereIf(predicate is not null, predicate!)
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(true)
            .WhereIf(predicate is not null, predicate!)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public ValueTask<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
            => DbSet.FindAsync([id], cancellationToken);

    /// <inheritdoc />
    public ValueTask<TEntity?> GetByIdAsync(object?[]? keyValues, CancellationToken cancellationToken = default)
            => DbSet.FindAsync(keyValues, cancellationToken);

    /// <summary>
    /// Decodes a base64 string back into a cursor.
    /// </summary>
    /// <typeparam name="TCursor">The type of the cursor.</typeparam>
    /// <param name="cursorString">The encoded cursor string.</param>
    /// <returns>The decoded cursor value, or default if invalid.</returns>
    public static TCursor? DecodeCursor<TCursor>(string? cursorString)
    {
        if (string.IsNullOrEmpty(cursorString))
            return default;

        try
        {
            var bytes = Convert.FromBase64String(cursorString);
            var json = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<TCursor>(json);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Gets a queryable for the entity, optionally with tracking disabled.
    /// </summary>
    /// <param name="asNoTracking">Whether to disable tracking.</param>
    /// <returns>The queryable.</returns>
    protected IQueryable<TEntity> GetQueryable(bool asNoTracking)
            => asNoTracking ? DbSet.AsNoTracking() : DbSet;

    /// <summary>
    /// Builds the expression for cursor based pagination.
    /// </summary>
    /// <typeparam name="TKey">The type of the cursor key.</typeparam>
    /// <param name="cursorSelector">The expression to select the cursor property.</param>
    /// <param name="cursor">The cursor value.</param>
    /// <param name="ascending">Whether to sort ascending.</param>
    /// <param name="direction">The cursor direction.</param>
    /// <returns>A boolean expression filtering items relative to the cursor.</returns>
    protected Expression<Func<TEntity, bool>> BuildCursorExpression<TKey>(
        Expression<Func<TEntity, TKey>> cursorSelector,
        TKey cursor,
        bool ascending,
        CursorDirection direction)
        where TKey : IComparable<TKey>
    {
        var parameter = cursorSelector.Parameters[0];
        var cursorValue = Expression.Constant(cursor, typeof(TKey));

        Expression comparison;

        if (direction == CursorDirection.Forward)
        {
            comparison = ascending
                ? Expression.GreaterThan(cursorSelector.Body, cursorValue)
                : Expression.LessThan(cursorSelector.Body, cursorValue);
        }
        else
        {
            comparison = ascending
                ? Expression.LessThan(cursorSelector.Body, cursorValue)
                : Expression.GreaterThan(cursorSelector.Body, cursorValue);
        }

        return Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
    }

    /// <summary>
    /// Gets a single entity based on the predicate and tracking options.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <param name="asNoTracking">Whether to disable tracking.</param>
    /// <param name="include">Related data to include.</param>
    /// <param name="useFirst">If true, uses FirstOrDefault; otherwise SingleOrDefault.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity or null.</returns>
    protected async Task<TEntity?> GetEntityInternalAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool asNoTracking,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool useFirst = true,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(asNoTracking);

        if (include is not null)
        {
            query = include(query);
        }

        query = query.Where(predicate);

        return useFirst
            ? await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)
            : await query.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a list of entities based on the predicate and tracking options.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <param name="asNoTracking">Whether to disable tracking.</param>
    /// <param name="include">Related data to include.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of entities.</returns>
    protected async Task<IReadOnlyList<TEntity>> GetListInternalAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool asNoTracking,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(asNoTracking);

        if (include is not null)
        {
            query = include(query);
        }

        return await query.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Private Methods

    private static string EncodeCursor<TCursor>(TCursor cursor)
    {
        var json = JsonSerializer.Serialize(cursor);
        var bytes = Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(bytes);
    }

    #endregion
}
