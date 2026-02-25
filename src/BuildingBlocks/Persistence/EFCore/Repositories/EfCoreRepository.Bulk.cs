using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using VK.Blocks.Persistence.Abstractions.Repositories;
using VK.Blocks.Persistence.EfCore.Internal;
using VK.Blocks.Persistence.EFCore.Caches;

namespace VK.Blocks.Persistence.EFCore.Repositories;

public partial class EfCoreRepository<TEntity>
{
#if NET8_0
    #region Public Methods

    /// <inheritdoc />
    public async Task<int> ExecuteUpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Action<IPropertySetter<TEntity>> setPropertyAction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(setPropertyAction);

        var propertySetter = new EfCorePropertySetter<TEntity>();
        setPropertyAction(propertySetter);

        // Bulk updates bypass the ChangeTracker, so Interceptors are NOT triggered.
        // We must manually invoke the processor to handle auditing fields (Validation is skipped).
        _processor.ProcessBulkUpdate(propertySetter);

        var setPropertyExpression = propertySetter.BuildSetPropertyExpression();
        var updatedRows = await DbSet.Where(predicate).ExecuteUpdateAsync(setPropertyExpression, cancellationToken);

        logger.LogInformation("Bulk update affected {Count} rows for {EntityType}", updatedRows, typeof(TEntity).Name);

        return updatedRows;
    }

    /// <inheritdoc />
    public async Task<int> ExecuteDeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool forceDelete = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var query = GetQueryable(false).Where(predicate);

        if (!forceDelete && EfCoreTypeCache<TEntity>.IsSoftDelete)
        {
            var propertySetter = new EfCorePropertySetter<TEntity>();

            // Bulk deletes bypass the ChangeTracker. For Soft Delete, we are actually doing an Update.
            // We must manually invoke the processor to set IsDeleted = true and generic auditing fields.
            _processor.ProcessBulkSoftDelete(propertySetter);

            var setPropertyExpression = propertySetter.BuildSetPropertyExpression();
            var softDeletedRows = await query.ExecuteUpdateAsync(setPropertyExpression, cancellationToken).ConfigureAwait(false);

            logger.LogInformation("Bulk softdelete affected {Count} rows for {EntityType}", softDeletedRows, typeof(TEntity).Name);

            return softDeletedRows;
        }

        var deletedRows = await query.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Bulk delete affected {Count} rows for {EntityType}", deletedRows, typeof(TEntity).Name);

        return deletedRows;
    }

    #endregion
#else

    #region Public Methods
    /// <inheritdoc />
    public async Task<int> ExecuteUpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Action<IPropertySetter<TEntity>> setPropertyAction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(setPropertyAction);

        var updatedRows = await DbSet.Where(predicate).ExecuteUpdateAsync(builder =>
        {
            var adapter = new EfCorePropertySetterAdapter<TEntity>(builder);
            setPropertyAction(adapter);

            // Bulk updates bypass the ChangeTracker, so Interceptors are NOT triggered.
            // We must manually invoke the processor to handle auditing fields (Validation is skipped).
            _processor.ProcessBulkUpdate(adapter);
        }, cancellationToken);

        logger.LogBulkUpdateSuccess(updatedRows, EfCoreTypeCache<TEntity>.EntityName);

        return updatedRows;
    }

    /// <inheritdoc />
    public async Task<int> ExecuteDeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool forceDelete = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var query = GetQueryable(false).Where(predicate);

        if (!forceDelete && EfCoreTypeCache<TEntity>.IsSoftDelete)
        {
            var softDeletedRows = await query.ExecuteUpdateAsync(builder =>
            {
                var adapter = new EfCorePropertySetterAdapter<TEntity>(builder);

                // Bulk deletes bypass the ChangeTracker. For Soft Delete, we are actually doing an Update.
                // We must manually invoke the processor to set IsDeleted = true and generic auditing fields.
                _processor.ProcessBulkSoftDelete(adapter);
            }, cancellationToken).ConfigureAwait(false);

            logger.LogBulkSoftDeleteSuccess(softDeletedRows, EfCoreTypeCache<TEntity>.EntityName);

            return softDeletedRows;
        }

        var deletedRows = await query.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

        logger.LogBulkDeleteSuccess(deletedRows, EfCoreTypeCache<TEntity>.EntityName);

        return deletedRows;
    }
    #endregion
#endif
}
