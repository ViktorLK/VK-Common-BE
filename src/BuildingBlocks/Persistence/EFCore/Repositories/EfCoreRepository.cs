using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VK.Blocks.Persistence.Core.Pagination;
using VK.Blocks.Persistence.Abstractions.Repositories;
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
/// <param name="cursorSerializer">
/// The cursor serializer. Defaults to <c>SimpleCursorSerializer</c> if not provided.
/// </param>
public partial class EfCoreRepository<TEntity>(
    DbContext context,
    ILogger<EfCoreRepository<TEntity>> logger,
    ICursorSerializer cursorSerializer,
    IEntityLifecycleProcessor processor
) : EfCoreReadRepository<TEntity>(context, logger, cursorSerializer), IBaseRepository<TEntity>
    where TEntity : class
{
    #region Fields

    /// <summary>
    /// The entity lifecycle processor.
    /// </summary>
    /// <remarks>
    /// This processor is ONLY used for Bulk Operations (<see cref="ExecuteUpdateAsync"/>, <see cref="ExecuteDeleteAsync"/>)
    /// which bypass the EF Core ChangeTracker and thus do not trigger Interceptors.
    /// Standard CRUD operations (Add, Update, Delete) are handled automatically by <see cref="VK.Blocks.Persistence.EFCore.Interceptors.AuditingInterceptor"/>.
    /// </remarks>
    protected readonly IEntityLifecycleProcessor _processor = processor;

    #endregion

    #region Public Methods

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

    #endregion
}
