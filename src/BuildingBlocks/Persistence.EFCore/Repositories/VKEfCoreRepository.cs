using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Implementation of the generic repository base class for EF Core.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public sealed partial class VKEfCoreRepository<TEntity>(
    DbContext context,
    ILogger<VKEfCoreRepository<TEntity>> logger,
    IVKCursorSerializer cursorSerializer,
    IVKEntityLifecycleProcessor processor
) : VKEfCoreReadRepository<TEntity>(context, logger, cursorSerializer), IVKBaseRepository<TEntity>
    where TEntity : class
{
    private readonly ILogger<VKEfCoreRepository<TEntity>> _logger = VKGuard.NotNull(logger);
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
    public ValueTask UpdateAsync(TEntity entity)
    {
        VKGuard.NotNull(entity);
        DbSet.Update(entity);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask UpdateRangeAsync(IReadOnlyList<TEntity> entities)
    {
        VKGuard.NotNull(entities);
        DbSet.UpdateRange(entities);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DeleteAsync(TEntity entity)
    {
        VKGuard.NotNull(entity);
        DbSet.Remove(entity);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DeleteRangeAsync(IReadOnlyList<TEntity> entities)
    {
        VKGuard.NotNull(entities);
        DbSet.RemoveRange(entities);
        return ValueTask.CompletedTask;
    }
}
