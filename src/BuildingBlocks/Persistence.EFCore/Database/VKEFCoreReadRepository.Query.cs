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

public partial class VKEFCoreReadRepository<TEntity>
{
    /// <inheritdoc />
    public virtual async Task<VKPagedResult<TEntity>> GetPagedAsync<TKey>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>> orderBy,
        int pageNumber = 1,
        int pageSize = 20,
        bool ascending = true,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(orderBy); // [AP.01]

        // Validation: Guard against invalid parameters and deep paging performance issues.
        var validationResult = VKPaginationValidator.ValidateOffsetPagination(pageNumber, pageSize);
        if (validationResult.IsFailure)
        {
            throw new VKValidationException(validationResult.Errors.Select(e => new VKValidationError(string.Empty, e.Description, e.Code)));
        }

        var query = GetQueryable(options).WhereIf(predicate is not null, predicate!);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false); // [CS.03]

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
                               .ConfigureAwait(false); // [CS.03]

        return new VKPagedResult<TEntity>
        {
            Items = items.AsReadOnly(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc />
    public virtual async Task<VKPagedResult<TResult>> GetPagedAsync<TKey, TResult>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>> orderBy,
        Expression<Func<TEntity, TResult>> selector,
        int pageNumber = 1,
        int pageSize = 20,
        bool ascending = true,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(orderBy); // [AP.01]
        VKGuard.NotNull(selector); // [AP.01]

        var validationResult = VKPaginationValidator.ValidateOffsetPagination(pageNumber, pageSize);
        if (validationResult.IsFailure)
        {
            throw new VKValidationException(validationResult.Errors.Select(e => new VKValidationError(string.Empty, e.Description, e.Code)));
        }

        var query = GetQueryable(options).WhereIf(predicate is not null, predicate!);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false); // [CS.03]

        if (totalCount == 0)
        {
            return new VKPagedResult<TResult>
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
                               .Select(selector)
                               .ToListAsync(cancellationToken)
                               .ConfigureAwait(false); // [CS.03]

        return new VKPagedResult<TResult>
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
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
        where TCursor : IComparable<TCursor>
    {
        VKGuard.NotNull(cursorSelector); // [AP.01]

        // Validation: Guard against excessively large page sizes.
        var validationResult = VKPaginationValidator.ValidateCursorPagination(pageSize);
        if (validationResult.IsFailure)
        {
            throw new VKValidationException(validationResult.Errors.Select(e => new VKValidationError(string.Empty, e.Description, e.Code)));
        }

        var hasCursor = !EqualityComparer<TCursor>.Default.Equals(cursor!, default!);

        // Rationale: We fetch one extra item to determine if there is a next page.
        var items = await GetQueryable(options)
            .WhereIf(predicate is not null, predicate!)
            .WhereIf(cursor is not null && hasCursor, BuildCursorExpression(cursorSelector, cursor!, ascending, direction))
            .OrderByVKCursorDirection(ascending, cursorSelector, direction)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false); // [CS.03]

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
    public virtual async Task<VKCursorPagedResult<TResult>> GetCursorPagedAsync<TCursor, TResult>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TCursor>> cursorSelector,
        Expression<Func<TEntity, TResult>> selector,
        TCursor? cursor = default,
        int pageSize = 20,
        bool ascending = true,
        VKCursorDirection direction = VKCursorDirection.Forward,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
        where TCursor : IComparable<TCursor>
    {
        VKGuard.NotNull(cursorSelector); // [AP.01]
        VKGuard.NotNull(selector); // [AP.01]

        var validationResult = VKPaginationValidator.ValidateCursorPagination(pageSize);
        if (validationResult.IsFailure)
        {
            throw new VKValidationException(validationResult.Errors.Select(e => new VKValidationError(string.Empty, e.Description, e.Code)));
        }

        var hasCursor = !EqualityComparer<TCursor>.Default.Equals(cursor!, default!);

        var items = await GetQueryable(options)
            .WhereIf(predicate is not null, predicate!)
            .WhereIf(cursor is not null && hasCursor, BuildCursorExpression(cursorSelector, cursor!, ascending, direction))
            .OrderByVKCursorDirection(ascending, cursorSelector, direction)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false); // [CS.03]

        var hasMore = items.Count > pageSize;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

        if (direction == VKCursorDirection.Backward)
        {
            items.Reverse();
        }

        var compiledSelector = VKExpressionCache.GetOrCompile(cursorSelector);
        var compiledProjector = selector.Compile();

        var nextCursorValue = hasMore && direction == VKCursorDirection.Forward && items.Count != 0 ? compiledSelector(items[^1]) : default;
        var previousCursorValue = hasMore && direction == VKCursorDirection.Backward && items.Count != 0 ? compiledSelector(items[0]) : default;

        var nextCursorString = nextCursorValue is not null && !EqualityComparer<TCursor>.Default.Equals(nextCursorValue, default!)
            ? CursorSerializer.Serialize(nextCursorValue)
            : null;

        var previousCursorString = previousCursorValue is not null && !EqualityComparer<TCursor>.Default.Equals(previousCursorValue, default!)
            ? CursorSerializer.Serialize(previousCursorValue)
            : null;

        var projectedItems = items.Select(compiledProjector).ToList();

        return new VKCursorPagedResult<TResult>
        {
            Items = projectedItems.AsReadOnly(),
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
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(options)
            .WhereIf(predicate is not null, predicate!)
            .AsAsyncEnumerable();
    }

    /// <inheritdoc />
    public virtual async Task<VKPagedResult<TEntity>> GetPagedAsync(
        IVKSpecification<TEntity> specification,
        int pageNumber = 1,
        int pageSize = 20,
        bool ascending = true,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(specification); // [AP.01]

        var validationResult = VKPaginationValidator.ValidateOffsetPagination(pageNumber, pageSize);
        if (validationResult.IsFailure)
        {
            throw new VKValidationException(validationResult.Errors.Select(e => new VKValidationError(string.Empty, e.Description, e.Code)));
        }

        var query = GetQueryable(options).ApplySpecification(specification);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false); // [CS.03]

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
        var items = await query.Skip(offset)
                               .Take(pageSize)
                               .ToListAsync(cancellationToken)
                               .ConfigureAwait(false); // [CS.03]

        return new VKPagedResult<TEntity>
        {
            Items = items.AsReadOnly(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc />
    public virtual async Task<VKPagedResult<TResult>> GetPagedAsync<TResult>(
        IVKSpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> selector,
        int pageNumber = 1,
        int pageSize = 20,
        bool ascending = true,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(specification); // [AP.01]
        VKGuard.NotNull(selector); // [AP.01]

        var validationResult = VKPaginationValidator.ValidateOffsetPagination(pageNumber, pageSize);
        if (validationResult.IsFailure)
        {
            throw new VKValidationException(validationResult.Errors.Select(e => new VKValidationError(string.Empty, e.Description, e.Code)));
        }

        var query = GetQueryable(options).ApplySpecification(specification);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false); // [CS.03]

        if (totalCount == 0)
        {
            return new VKPagedResult<TResult>
            {
                Items = [],
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0
            };
        }

        var offset = (pageNumber - 1) * pageSize;
        var items = await query.Skip(offset)
                               .Take(pageSize)
                               .Select(selector)
                               .ToListAsync(cancellationToken)
                               .ConfigureAwait(false); // [CS.03]

        return new VKPagedResult<TResult>
        {
            Items = items.AsReadOnly(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc />
    public virtual async Task<VKCursorPagedResult<TEntity>> GetCursorPagedAsync<TCursor>(
        IVKSpecification<TEntity> specification,
        Expression<Func<TEntity, TCursor>> cursorSelector,
        TCursor? cursor = default,
        int pageSize = 20,
        bool ascending = true,
        VKCursorDirection direction = VKCursorDirection.Forward,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
        where TCursor : IComparable<TCursor>
    {
        VKGuard.NotNull(specification); // [AP.01]
        VKGuard.NotNull(cursorSelector); // [AP.01]

        var validationResult = VKPaginationValidator.ValidateCursorPagination(pageSize);
        if (validationResult.IsFailure)
        {
            throw new VKValidationException(validationResult.Errors.Select(e => new VKValidationError(string.Empty, e.Description, e.Code)));
        }

        var hasCursor = !EqualityComparer<TCursor>.Default.Equals(cursor!, default!);

        var items = await GetQueryable(options)
            .ApplySpecification(specification)
            .WhereIf(cursor is not null && hasCursor, BuildCursorExpression(cursorSelector, cursor!, ascending, direction))
            .OrderByVKCursorDirection(ascending, cursorSelector, direction)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false); // [CS.03]

        var hasMore = items.Count > pageSize;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

        if (direction == VKCursorDirection.Backward)
        {
            items.Reverse();
        }

        var compiledSelector = VKExpressionCache.GetOrCompile(cursorSelector);

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
    public virtual async Task<VKCursorPagedResult<TResult>> GetCursorPagedAsync<TCursor, TResult>(
        IVKSpecification<TEntity> specification,
        Expression<Func<TEntity, TCursor>> cursorSelector,
        Expression<Func<TEntity, TResult>> selector,
        TCursor? cursor = default,
        int pageSize = 20,
        bool ascending = true,
        VKCursorDirection direction = VKCursorDirection.Forward,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
        where TCursor : IComparable<TCursor>
    {
        VKGuard.NotNull(specification); // [AP.01]
        VKGuard.NotNull(cursorSelector); // [AP.01]
        VKGuard.NotNull(selector); // [AP.01]

        var validationResult = VKPaginationValidator.ValidateCursorPagination(pageSize);
        if (validationResult.IsFailure)
        {
            throw new VKValidationException(validationResult.Errors.Select(e => new VKValidationError(string.Empty, e.Description, e.Code)));
        }

        var hasCursor = !EqualityComparer<TCursor>.Default.Equals(cursor!, default!);

        var items = await GetQueryable(options)
            .ApplySpecification(specification)
            .WhereIf(cursor is not null && hasCursor, BuildCursorExpression(cursorSelector, cursor!, ascending, direction))
            .OrderByVKCursorDirection(ascending, cursorSelector, direction)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false); // [CS.03]

        var hasMore = items.Count > pageSize;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

        if (direction == VKCursorDirection.Backward)
        {
            items.Reverse();
        }

        var compiledSelector = VKExpressionCache.GetOrCompile(cursorSelector);
        var compiledProjector = selector.Compile();

        var nextCursorValue = hasMore && direction == VKCursorDirection.Forward && items.Count != 0 ? compiledSelector(items[^1]) : default;
        var previousCursorValue = hasMore && direction == VKCursorDirection.Backward && items.Count != 0 ? compiledSelector(items[0]) : default;

        var nextCursorString = nextCursorValue is not null && !EqualityComparer<TCursor>.Default.Equals(nextCursorValue, default!)
            ? CursorSerializer.Serialize(nextCursorValue)
            : null;

        var previousCursorString = previousCursorValue is not null && !EqualityComparer<TCursor>.Default.Equals(previousCursorValue, default!)
            ? CursorSerializer.Serialize(previousCursorValue)
            : null;

        var projectedItems = items.Select(compiledProjector).ToList();

        return new VKCursorPagedResult<TResult>
        {
            Items = projectedItems.AsReadOnly(),
            NextCursor = nextCursorString,
            PreviousCursor = previousCursorString,
            HasNextPage = direction == VKCursorDirection.Forward ? hasMore : cursor is not null && hasCursor,
            HasPreviousPage = direction == VKCursorDirection.Backward ? hasMore : cursor is not null && hasCursor,
            PageSize = pageSize
        };
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
