using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore.Interceptors.Internal;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Implementation of the generic repository base class for EF Core.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public partial class VKEFCoreRepository<TEntity>(
    DbContext context,
    ILogger<VKEFCoreRepository<TEntity>> logger,
    IVKCursorSerializer cursorSerializer,
    IVKEntityLifecycleProcessor processor,
    IEnumerable<IVKQueryContributor>? queryContributors = null
) : VKEFCoreReadRepository<TEntity>(context, logger, cursorSerializer, queryContributors), IVKEntityRepository<TEntity>, IVKEntityBulkRepository<TEntity>
    where TEntity : class
{
    private readonly ILogger<VKEFCoreRepository<TEntity>> _logger = VKGuard.NotNull(logger);
    private readonly IVKEntityLifecycleProcessor _processor = VKGuard.NotNull(processor);

    /// <inheritdoc />
    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entity);

        var entry = await DbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        return entry.Entity;
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entities);

        await DbSet.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public ValueTask UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        VKGuard.NotNull(entity);
        DbSet.Update(entity);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask UpdateRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        VKGuard.NotNull(entities);
        DbSet.UpdateRange(entities);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        VKGuard.NotNull(entity);
        DbSet.Remove(entity);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DeleteRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        VKGuard.NotNull(entities);
        DbSet.RemoveRange(entities);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask HardDeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        VKGuard.NotNull(entity);
        PhysicalDeleteRegistry.Register(entity);
        DbSet.Remove(entity);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask HardDeleteRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        VKGuard.NotNull(entities);
        foreach (var entity in entities)
        {
            PhysicalDeleteRegistry.Register(entity);
        }
        DbSet.RemoveRange(entities);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<TEntity> UpsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entity); // [AP.01]

        var keyValues = Context.Entry(entity).Metadata.FindPrimaryKey()?.Properties
            .Select(p => p.GetGetter().GetClrValue(entity))
            .ToArray();

        if (keyValues is null || keyValues.Length == 0 || keyValues.Any(v => v is null)) // [AP.01]
        {
            var entry = await DbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false); // [CS.03]
            return entry.Entity;
        }

        var dbEntity = await DbSet.FindAsync(keyValues, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (dbEntity is null) // [AP.01]
        {
            var entry = await DbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false); // [CS.03]
            return entry.Entity;
        }
        else
        {
            Context.Entry(dbEntity).CurrentValues.SetValues(entity);
            return dbEntity;
        }
    }
}
