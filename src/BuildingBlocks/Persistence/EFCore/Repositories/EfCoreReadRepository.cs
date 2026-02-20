using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VK.Blocks.Validation;
using VK.Blocks.Persistence.Core.Pagination;
using VK.Blocks.Core.Results;
using VK.Blocks.Persistence.Abstractions.Repositories;
using VK.Blocks.Persistence.EFCore.Extensions;
using VK.Blocks.Persistence.EFCore.Infrastructure;

namespace VK.Blocks.Persistence.EFCore.Repositories;

/// <summary>
/// Implementation of the generic read-only repository base class.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public partial class EfCoreReadRepository<TEntity> : IReadRepository<TEntity>
    where TEntity : class
{
    #region Fields

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
    protected readonly ICursorSerializer CursorSerializer;

    /// <summary>
    /// The logger instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="EfCoreReadRepository{TEntity}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cursorSerializer">
    /// The cursor serializer. Defaults to <see cref="SimpleCursorSerializer"/> if not provided.
    /// </param>
    public EfCoreReadRepository(DbContext context, ILogger logger, ICursorSerializer cursorSerializer)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cursorSerializer);

        Context = context;
        DbSet = context.Set<TEntity>();
        _logger = logger;
        CursorSerializer = cursorSerializer;
    }

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
        ArgumentNullException.ThrowIfNull(builder);
        return await builder(GetQueryable(true)).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TResult?> ExecuteSingleAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return await builder(GetQueryable(true)).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
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

    #endregion

    #region Protected Methods

    /// <summary>
    /// Gets a queryable for the entity, optionally with tracking disabled.
    /// </summary>
    /// <param name="asNoTracking">Whether to disable tracking.</param>
    /// <returns>The queryable.</returns>
    protected IQueryable<TEntity> GetQueryable(bool asNoTracking)
            => asNoTracking ? DbSet.AsNoTracking() : DbSet;

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
}
