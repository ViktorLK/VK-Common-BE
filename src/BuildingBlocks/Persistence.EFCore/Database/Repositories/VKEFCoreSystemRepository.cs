using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// EF Core implementation of <see cref="IVKSystemRepository{TEntity}"/>.
/// Automatically forces <see cref="EntityFrameworkQueryableExtensions.IgnoreQueryFilters{TEntity}"/> for all read operations
/// and supports system-level cross-tenant operations. (AP.01)
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public sealed class VKEFCoreSystemRepository<TEntity>(
    DbContext context,
    ILogger<VKEFCoreRepository<TEntity>> logger,
    IVKCursorSerializer cursorSerializer,
    IVKEntityLifecycleProcessor processor
) : VKEFCoreReadRepository<TEntity>(context, logger, cursorSerializer), IVKSystemRepository<TEntity>
    where TEntity : class
{
    private readonly VKEFCoreRepository<TEntity> _innerWriteRepository = new(context, logger, cursorSerializer, processor);

    /// <inheritdoc />
    protected override IQueryable<TEntity> GetQueryable(bool asNoTracking)
    {
        return base.GetQueryable(asNoTracking).IgnoreQueryFilters();
    }

    /// <inheritdoc />
    protected override IQueryable<TEntity> GetQueryable(VKQueryOptions? options)
    {
        return base.GetQueryable(options).IgnoreQueryFilters();
    }

    /// <inheritdoc />
    protected override IQueryable<TEntity> ApplyOptions(IQueryable<TEntity> query, VKQueryOptions? options)
    {
        var resultQuery = base.ApplyOptions(query, options);
        // System repository always ignores global query filters (e.g. MultiTenancy query filter)
        return resultQuery.IgnoreQueryFilters();
    }

    /// <inheritdoc />
    public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => _innerWriteRepository.AddAsync(entity, cancellationToken);

    /// <inheritdoc />
    public Task AddRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
        => _innerWriteRepository.AddRangeAsync(entities, cancellationToken);

    /// <inheritdoc />
    public ValueTask UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        => _innerWriteRepository.UpdateAsync(entity, cancellationToken);

    /// <inheritdoc />
    public ValueTask UpdateRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
        => _innerWriteRepository.UpdateRangeAsync(entities, cancellationToken);

    /// <inheritdoc />
    public Task<TEntity> UpsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        => _innerWriteRepository.UpsertAsync(entity, cancellationToken);

    /// <inheritdoc />
    public ValueTask DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        => _innerWriteRepository.DeleteAsync(entity, cancellationToken);

    /// <inheritdoc />
    public ValueTask DeleteRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
        => _innerWriteRepository.DeleteRangeAsync(entities, cancellationToken);

    /// <inheritdoc />
    public ValueTask HardDeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        => _innerWriteRepository.HardDeleteAsync(entity, cancellationToken);

    /// <inheritdoc />
    public ValueTask HardDeleteRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
        => _innerWriteRepository.HardDeleteRangeAsync(entities, cancellationToken);
}
