using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Implementation of the generic read-only repository base class for EF Core.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public partial class VKEFCoreReadRepository<TEntity> : IVKReadRepository<TEntity>
    where TEntity : class
{
    private readonly ILogger _logger;

    /// <summary>
    /// The database context.
    /// </summary>
    protected readonly DbContext Context;

    /// <summary>
    /// The DB set for the entity.
    /// </summary>
    protected readonly DbSet<TEntity> DbSet;

    /// <summary>
    /// The cursor serializer used to encode and decode cursor tokens.
    /// </summary>
    protected readonly IVKCursorSerializer CursorSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKEFCoreReadRepository{TEntity}"/> class.
    /// </summary>
    public VKEFCoreReadRepository(DbContext context, ILogger logger, IVKCursorSerializer cursorSerializer)
    {
        Context = VKGuard.NotNull(context);
        _logger = VKGuard.NotNull(logger);
        CursorSerializer = VKGuard.NotNull(cursorSerializer);
        DbSet = context.Set<TEntity>();
    }

    // =========================================================================
    // 1. Basic Single/Exists & Key-based Lookup
    // =========================================================================

    /// <inheritdoc />
    public ValueTask<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
            => DbSet.FindAsync([id], cancellationToken);

    /// <inheritdoc />
    public ValueTask<TEntity?> GetByIdAsync(object?[]? keyValues, CancellationToken cancellationToken = default)
            => DbSet.FindAsync(keyValues, cancellationToken);

    /// <inheritdoc />
    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return await GetQueryable(options)
            .WhereIf(predicate is not null, predicate!) // [AP.01]
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false); // [CS.03]
    }

    /// <inheritdoc />
    public async Task<bool> AnyAsync(
        IVKSpecification<TEntity> specification,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(specification); // [AP.01]
        return await GetQueryable(options)
            .ApplySpecification(specification)
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false); // [CS.03]
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return await GetQueryable(options)
            .WhereIf(predicate is not null, predicate!) // [AP.01]
            .CountAsync(cancellationToken)
            .ConfigureAwait(false); // [CS.03]
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        IVKSpecification<TEntity> specification,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(specification); // [AP.01]
        return await GetQueryable(options)
            .ApplySpecification(specification)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false); // [CS.03]
    }

    // =========================================================================
    // 2. Default Read Queries (NoTracking by default)
    // =========================================================================

    /// <inheritdoc />
    public Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
            => GetEntityInternalAsync(predicate, include, options, true, cancellationToken);

    /// <inheritdoc />
    public async Task<TEntity?> GetFirstOrDefaultAsync(
        IVKSpecification<TEntity> specification,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(specification); // [AP.01]
        return await GetQueryable(options)
            .ApplySpecification(specification)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false); // [CS.03]
    }

    /// <inheritdoc />
    public Task<TEntity?> GetSingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
            => GetEntityInternalAsync(predicate, include, options, false, cancellationToken);

    /// <inheritdoc />
    public async Task<TEntity?> GetSingleOrDefaultAsync(
        IVKSpecification<TEntity> specification,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(specification); // [AP.01]
        return await GetQueryable(options)
            .ApplySpecification(specification)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false); // [CS.03]
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
            => GetListInternalAsync(predicate, include, options, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> GetListAsync(
        IVKSpecification<TEntity> specification,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(specification); // [AP.01]
        return await GetQueryable(options)
            .ApplySpecification(specification)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false); // [CS.03]
    }

    // =========================================================================
    // 3. Tracked Read Queries (ChangeTracker Enabled)
    // =========================================================================

    /// <inheritdoc />
    public ValueTask<TEntity?> GetTrackedByIdAsync(object id, CancellationToken cancellationToken = default)
            => GetByIdAsync(id, cancellationToken);

    /// <inheritdoc />
    public ValueTask<TEntity?> GetTrackedByIdAsync(object?[]? keyValues, CancellationToken cancellationToken = default)
            => GetByIdAsync(keyValues, cancellationToken);

    /// <inheritdoc />
    public Task<TEntity?> GetTrackedFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
            => GetFirstOrDefaultAsync(predicate, include, new VKQueryOptions { Tracking = VKQueryTracking.Tracked }, cancellationToken);

    /// <inheritdoc />
    public Task<TEntity?> GetTrackedFirstOrDefaultAsync(
        IVKSpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
            => GetFirstOrDefaultAsync(specification, new VKQueryOptions { Tracking = VKQueryTracking.Tracked }, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<TEntity>> GetTrackedListAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
            => GetListAsync(predicate, include, new VKQueryOptions { Tracking = VKQueryTracking.Tracked }, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<TEntity>> GetTrackedListAsync(
        IVKSpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
            => GetListAsync(specification, new VKQueryOptions { Tracking = VKQueryTracking.Tracked }, cancellationToken);

    // =========================================================================
    // 4. Advanced Projection & Stream Queries
    // =========================================================================

    /// <inheritdoc />
    public async Task<TResult?> QuerySingleAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(builder); // [AP.01]
        var query = GetQueryable(options);
        return await builder(query).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false); // [CS.03]
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TResult>> QueryAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(builder); // [AP.01]
        var query = GetQueryable(options);
        return await builder(query).ToListAsync(cancellationToken).ConfigureAwait(false); // [CS.03]
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> FromSqlRawAsync(
        string sql,
        CancellationToken cancellationToken = default,
        params object[] parameters)
    {
        return await DbSet
            .FromSqlRaw(sql, parameters)
            .ToListAsync(cancellationToken).ConfigureAwait(false); // [CS.03]
    }

    // =========================================================================
    // 5. Internal Helper Methods & Infrastructure
    // =========================================================================

    /// <summary>
    /// Gets a queryable for the entity, optionally with tracking disabled.
    /// </summary>
    protected virtual IQueryable<TEntity> GetQueryable(bool asNoTracking)
            => asNoTracking ? DbSet.AsNoTracking() : DbSet.AsTracking();

    /// <summary>
    /// Gets a queryable for the entity, applying the specified query options.
    /// </summary>
    protected virtual IQueryable<TEntity> GetQueryable(VKQueryOptions? options)
    {
        var query = DbSet.AsQueryable();
        var tracking = options?.Tracking ?? VKQueryTracking.Default; // [AP.01]
        query = tracking switch
        {
            VKQueryTracking.Tracked => query.AsTracking(),
            _ => query.AsNoTracking()
        };
        return ApplyOptions(query, options);
    }

    /// <summary>
    /// Applies the query options to the queryable.
    /// </summary>
    protected virtual IQueryable<TEntity> ApplyOptions(IQueryable<TEntity> query, VKQueryOptions? options)
    {
        if (options is null) // [AP.01]
        {
            return query;
        }

        if (options.IgnoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (!string.IsNullOrWhiteSpace(options.QueryTag))
        {
            query = query.TagWith(options.QueryTag);
        }

        if (options.SplitQuery)
        {
            query = query.AsSplitQuery();
        }

        return query;
    }

    /// <summary>
    /// Gets a single entity based on the predicate and options.
    /// </summary>
    protected async Task<TEntity?> GetEntityInternalAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        VKQueryOptions? options = null,
        bool useFirst = true,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(options);

        if (include is not null) // [AP.01]
        {
            query = include(query);
        }

        query = query.Where(predicate);

        return useFirst
            ? await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) // [CS.03]
            : await query.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false); // [CS.03]
    }

    /// <summary>
    /// Gets a list of entities based on the predicate and options.
    /// </summary>
    protected async Task<IReadOnlyList<TEntity>> GetListInternalAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        VKQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(options);

        if (include is not null) // [AP.01]
        {
            query = include(query);
        }

        return await query.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false); // [CS.03]
    }
}
