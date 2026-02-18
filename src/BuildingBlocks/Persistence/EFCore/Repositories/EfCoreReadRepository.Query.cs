using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Common;
using VK.Blocks.Persistence.Abstractions.Pagination;
using VK.Blocks.Persistence.EFCore.Caches;
using VK.Blocks.Persistence.EFCore.Extensions;
using VK.Blocks.Persistence.EFCore.Infrastructure;

namespace VK.Blocks.Persistence.EFCore.Repositories;

public partial class EfCoreReadRepository<TEntity>
{
    #region Public Methods

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
                Items = [],
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

        var nextCursorString = nextCursor is not null && !EqualityComparer<TCursor>.Default.Equals(nextCursor, default)
            ? CursorSerializer.Serialize(nextCursor)
            : null;

        var previousCursorString = previousCursor is not null && !EqualityComparer<TCursor>.Default.Equals(previousCursor, default)
            ? CursorSerializer.Serialize(previousCursor)
            : null;

        return new CursorPagedResult<TEntity>
        {
            Items = items.AsReadOnly(),
            NextCursor = nextCursorString,
            PreviousCursor = previousCursorString,
            HasNextPage = direction == CursorDirection.Forward ? hasMore : cursor is not null && hasCursor,
            HasPreviousPage = direction == CursorDirection.Backward ? hasMore : cursor is not null && hasCursor,
            PageSize = pageSize
        };
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

    #endregion


    #region Protected Methods

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

    #endregion
}
