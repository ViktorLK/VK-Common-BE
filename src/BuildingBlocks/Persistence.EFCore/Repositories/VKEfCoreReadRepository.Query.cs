using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core;
using VK.Blocks.Validation;

using VKValidationException = VK.Blocks.Validation.VKValidationException;

namespace VK.Blocks.Persistence.EFCore;

public partial class VKEfCoreReadRepository<TEntity>
{
    /// <inheritdoc />
    public virtual async Task<VKPagedResult<TEntity>> GetPagedAsync<TKey>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>> orderBy,
        int pageNumber = 1,
        int pageSize = 20,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(orderBy);

        // Validation: Guard against invalid parameters and deep paging performance issues.
        var validationResult = VKPaginationValidator.ValidateOffsetPagination(pageNumber, pageSize);
        if (validationResult.IsFailure)
        {
            throw new VKValidationException(validationResult.Errors.Select(e => new VKValidationError(string.Empty, e.Description, e.Code)));
        }

        var query = GetQueryable(true).WhereIf(predicate is not null, predicate!);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        // Optimization: Early return if no items match.
        if (totalCount == 0)
        {
            return new VKPagedResult<TEntity>
            {
                Items = [],
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0
            };
        }

        var offset = (pageNumber - 1) * pageSize;
        var items = await query.OrderByIf(true, orderBy, ascending)
                               .Skip(offset)
                               .Take(pageSize)
                               .ToListAsync(cancellationToken)
                               .ConfigureAwait(false);

        return new VKPagedResult<TEntity>
        {
            Items = items.AsReadOnly(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc />
    public virtual async Task<VKCursorPagedResult<TEntity>> GetCursorPagedAsync<TCursor>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TCursor>> cursorSelector,
        TCursor? cursor = default,
        int pageSize = 20,
        bool ascending = true,
        VKCursorDirection direction = VKCursorDirection.Forward,
        CancellationToken cancellationToken = default)
        where TCursor : IComparable<TCursor>
    {
        VKGuard.NotNull(cursorSelector);

        // Validation: Guard against excessively large page sizes.
        var validationResult = VKPaginationValidator.ValidateCursorPagination(pageSize);
        if (validationResult.IsFailure)
        {
            throw new VKValidationException(validationResult.Errors.Select(e => new VKValidationError(string.Empty, e.Description, e.Code)));
        }

        var hasCursor = !EqualityComparer<TCursor>.Default.Equals(cursor!, default!);

        // Rationale: We fetch one extra item to determine if there is a next page.
        var items = await GetQueryable(true)
            .WhereIf(predicate is not null, predicate!)
            .WhereIf(cursor is not null && hasCursor, BuildCursorExpression(cursorSelector, cursor!, ascending, direction))
            .OrderByVKCursorDirection(ascending, cursorSelector, direction)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var hasMore = items.Count > pageSize;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

        if (direction == VKCursorDirection.Backward)
        {
            items.Reverse();
        }

        // Cache the compiled selector for performance (AP.01/CS.04).
        var compiledSelector = VKExpressionCache.GetOrCompile(cursorSelector);

        // Metadata logic for Next/Previous cursors and flags.
        var nextCursorValue = hasMore && direction == VKCursorDirection.Forward && items.Count != 0 ? compiledSelector(items[^1]) : default;
        var previousCursorValue = hasMore && direction == VKCursorDirection.Backward && items.Count != 0 ? compiledSelector(items[0]) : default;

        var nextCursorString = nextCursorValue is not null && !EqualityComparer<TCursor>.Default.Equals(nextCursorValue, default!)
            ? CursorSerializer.Serialize(nextCursorValue)
            : null;

        var previousCursorString = previousCursorValue is not null && !EqualityComparer<TCursor>.Default.Equals(previousCursorValue, default!)
            ? CursorSerializer.Serialize(previousCursorValue)
            : null;

        return new VKCursorPagedResult<TEntity>
        {
            Items = items.AsReadOnly(),
            NextCursor = nextCursorString,
            PreviousCursor = previousCursorString,
            HasNextPage = direction == VKCursorDirection.Forward ? hasMore : cursor is not null && hasCursor,
            HasPreviousPage = direction == VKCursorDirection.Backward ? hasMore : cursor is not null && hasCursor,
            PageSize = pageSize
        };
    }

    /// <inheritdoc />
    public virtual IAsyncEnumerable<TEntity> StreamAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(true)
            .WhereIf(predicate is not null, predicate!)
            .AsAsyncEnumerable();
    }

    /// <summary>
    /// Builds the expression for cursor based pagination.
    /// </summary>
    protected Expression<Func<TEntity, bool>> BuildCursorExpression<TCursor>(
        Expression<Func<TEntity, TCursor>> cursorSelector,
        TCursor cursor,
        bool ascending,
        VKCursorDirection direction)
        where TCursor : IComparable<TCursor>
    {
        var parameter = cursorSelector.Parameters[0];
        var cursorValue = Expression.Constant(cursor, typeof(TCursor));

        Expression comparison;

        if (direction == VKCursorDirection.Forward)
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
}

