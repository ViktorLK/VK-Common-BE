using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Persistence.EFCore.Extensions;
using VK.Blocks.Common;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// 汎用リポジトリ基底クラスの実装
/// </summary>
/// <typeparam name="TEntity">エンティティ型</typeparam>
public class BaseRepository<TEntity>(DbContext context) : IBaseRepository<TEntity>
    where TEntity : class
{

    public async Task<IReadOnlyList<TResult>> ExecuteAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        CancellationToken cancellationToken = default)
    {
        return await builder(GetQueryable(true)).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<TResult?> ExecuteSingleAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        CancellationToken cancellationToken = default)
    {
        return await builder(GetQueryable(true)).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    protected readonly DbContext Context = context ?? throw new ArgumentNullException(nameof(context));
    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

    protected IQueryable<TEntity> GetQueryable(bool asNoTracking)
            => asNoTracking ? DbSet.AsNoTracking() : DbSet;
    public virtual Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
            => GetEntityInternalAsync(predicate, false, include, true, cancellationToken);

    public virtual Task<TEntity?> GetFirstOrDefaultAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
            => GetEntityInternalAsync(predicate, true, include, true, cancellationToken);

    public virtual Task<TEntity?> GetSingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
            => GetEntityInternalAsync(predicate, false, include, false, cancellationToken);

    public virtual Task<TEntity?> GetSingleOrDefaultAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
            => GetEntityInternalAsync(predicate, true, include, false, cancellationToken);

    protected virtual async Task<TEntity?> GetEntityInternalAsync(
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

        return useFirst
            ? await query.Where(predicate).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)
            : await query.Where(predicate).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual Task<IReadOnlyList<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
            => GetListInternalAsync(predicate, false, include, cancellationToken);

    public virtual Task<IReadOnlyList<TEntity>> GetListAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
            => GetListInternalAsync(predicate, true, include, cancellationToken);

    protected virtual async Task<IReadOnlyList<TEntity>> GetListInternalAsync(
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

    public virtual async Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPagedAsync<TKey>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>> orderBy,
        int pageNumber,
        int pageSize,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        PaginationValidator.ValidateOffsetPagination(pageNumber, pageSize);

        var offset = (pageNumber - 1) * pageSize;

        var query = GetQueryable(true);

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        query = ascending
            ? query.OrderBy(orderBy)
            : query.OrderByDescending(orderBy);

        var items = await query
            .Skip(offset)
            .Take(pageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return (items, totalCount);
    }

    /// <summary>
    /// cursorProperty必须有索引且唯一
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="predicate"></param>
    /// <param name="cursorProperty"></param>
    /// <param name="lastValue"></param>
    /// <param name="pageSize"></param>
    /// <param name="ascending"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<CursorPagedResult<TEntity>> GetCursorPagedAsync<TKey>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>> cursorProperty,
        TKey? lastValue,
        int pageSize,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        var items = await GetQueryable(true)
            .WhereIf(predicate is not null, predicate!)
            .WhereIf(lastValue is not null, BuildCursorExpression(cursorProperty, lastValue!, ascending))
            .OrderByIf(cursorProperty is not null, cursorProperty!, ascending)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var hasNextPage = items.Count > pageSize;

        var resultItems = hasNextPage ? [.. items.Take(pageSize)] : items;

        return new CursorPagedResult<TEntity>
        {
            Items = resultItems,
            HasNextPage = hasNextPage,
            NextCursor = hasNextPage ? GetValue(resultItems[^1], cursorProperty)?.ToString() : null
        };
    }

    protected Expression<Func<TEntity, bool>> BuildCursorExpression<TKey>(
    Expression<Func<TEntity, TKey>> cursorProperty,
    TKey lastValue,
    bool ascending)
    {
        var parameter = cursorProperty.Parameters[0];

        var property = cursorProperty.Body;

        var constant = Expression.Constant(lastValue, typeof(TKey));

        var comparison = ascending
            ? Expression.GreaterThan(property, constant)
            : Expression.LessThan(property, constant);

        return Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
    }

    protected TKey? GetValue<TKey>(TEntity entity, Expression<Func<TEntity, TKey>> property)
    {
        return property.Compile()(entity);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entry = await DbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        return entry.Entity;
    }

    public virtual async Task AddRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        await DbSet.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
    }

    public virtual void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        DbSet.Update(entity);
    }

    public virtual void UpdateRange(IReadOnlyList<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        DbSet.UpdateRange(entities);
    }

    public virtual void Delete(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        DbSet.Remove(entity);
    }

    public virtual async Task<int> ExecuteDeleteRangeAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await GetQueryable(false)
            .Where(predicate)
            .ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<IReadOnlyList<TEntity>> FromSqlRawAsync(
        string sql,
        CancellationToken cancellationToken = default,
        params object[] parameters)
    {
        return await DbSet
            .FromSqlRaw(sql, parameters)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// ストリーミング検索 - 大量データシナリオに適用
    /// </summary>
    public virtual async IAsyncEnumerable<TEntity> StreamAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(true).WhereIf(predicate is not null, predicate!);

        await foreach (var entity in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return entity;
        }
    }
}
