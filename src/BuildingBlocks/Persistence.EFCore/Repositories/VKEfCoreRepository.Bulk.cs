using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore.BulkOperations.Internal;
using VK.Blocks.Persistence.EFCore.Diagnostics.Internal;

namespace VK.Blocks.Persistence.EFCore;

public partial class VKEfCoreRepository<TEntity>
{
#if NET8_0
    /// <inheritdoc />
    public async Task<int> ExecuteUpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Action<IVKPropertySetter<TEntity>> setPropertyAction,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(predicate);
        VKGuard.NotNull(setPropertyAction);

        var propertySetter = new VKEfCorePropertySetter<TEntity>();
        setPropertyAction(propertySetter);

        // Bulk updates bypass the ChangeTracker, so Interceptors are NOT triggered.
        // We must manually invoke the processor to handle auditing fields (Validation is skipped).
        _processor.ProcessBulkUpdate(propertySetter);

        var setPropertyExpression = propertySetter.BuildSetPropertyExpression();
        var updatedRows = await DbSet.Where(predicate).ExecuteUpdateAsync(setPropertyExpression, cancellationToken).ConfigureAwait(false);

        _logger.LogBulkUpdateSuccess(updatedRows, VKTypeMetadataCache.GetName<TEntity>());

        return updatedRows;
    }

    /// <inheritdoc />
    public async Task<int> ExecuteDeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool forceDelete = false,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(predicate);

        var query = GetQueryable(false).Where(predicate);

        if (!forceDelete && VKTypeMetadataCache.IsSoftDelete<TEntity>())
        {
            var propertySetter = new VKEfCorePropertySetter<TEntity>();

            // Bulk deletes bypass the ChangeTracker. For Soft Delete, we are actually doing an Update.
            // We must manually invoke the processor to set IsDeleted = true and generic auditing fields.
            _processor.ProcessBulkSoftDelete(propertySetter);

            var setPropertyExpression = propertySetter.BuildSetPropertyExpression();
            var softDeletedRows = await query.ExecuteUpdateAsync(setPropertyExpression, cancellationToken).ConfigureAwait(false);

            _logger.LogBulkSoftDeleteSuccess(softDeletedRows, VKTypeMetadataCache.GetName<TEntity>());

            return softDeletedRows;
        }

        var deletedRows = await query.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogBulkDeleteSuccess(deletedRows, VKTypeMetadataCache.GetName<TEntity>());

        return deletedRows;
    }
#else
    /// <inheritdoc />
    public async Task<int> ExecuteUpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Action<IVKPropertySetter<TEntity>> setPropertyAction,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(predicate);
        VKGuard.NotNull(setPropertyAction);

        var updatedRows = await DbSet.Where(predicate).ExecuteUpdateAsync(builder =>
        {
            var adapter = new EfCorePropertySetterAdapter<TEntity>(builder);
            setPropertyAction(adapter);

            // Bulk updates bypass the ChangeTracker, so Interceptors are NOT triggered.
            // We must manually invoke the processor to handle auditing fields (Validation is skipped).
            _processor.ProcessBulkUpdate(adapter);
        }, cancellationToken).ConfigureAwait(false);

        _logger.LogBulkUpdateSuccess(updatedRows, VKTypeMetadataCache.GetName<TEntity>());

        return updatedRows;
    }

    /// <inheritdoc />
    public async Task<int> ExecuteDeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool forceDelete = false,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(predicate);

        var query = GetQueryable(false).Where(predicate);

        if (!forceDelete && VKTypeMetadataCache.IsSoftDelete<TEntity>())
        {
            var softDeletedRows = await query.ExecuteUpdateAsync(builder =>
            {
                var adapter = new EfCorePropertySetterAdapter<TEntity>(builder);

                // Bulk deletes bypass the ChangeTracker. For Soft Delete, we are actually doing an Update.
                // We must manually invoke the processor to set IsDeleted = true and generic auditing fields.
                _processor.ProcessBulkSoftDelete(adapter);
            }, cancellationToken).ConfigureAwait(false);

            _logger.LogBulkSoftDeleteSuccess(softDeletedRows, VKTypeMetadataCache.GetName<TEntity>());

            return softDeletedRows;
        }

        var deletedRows = await query.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogBulkDeleteSuccess(deletedRows, VKTypeMetadataCache.GetName<TEntity>());

        return deletedRows;
    }
#endif
}
