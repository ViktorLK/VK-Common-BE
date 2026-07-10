using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore.Database.Internal;
using VK.Blocks.Persistence.EFCore.Diagnostics.Internal;

namespace VK.Blocks.Persistence.EFCore;

public partial class VKEFCoreRepository<TEntity>
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

        var propertySetter = new VKEFCorePropertySetter<TEntity>();
        setPropertyAction(propertySetter);

        // Bulk updates bypass the ChangeTracker, so Interceptors are NOT triggered.
        // We must manually invoke the processor to handle auditing fields (Validation is skipped).
        _processor.ProcessBulkUpdate(propertySetter);

        var setPropertyExpression = propertySetter.BuildSetPropertyExpression();
        var updatedRows = await DbSet.Where(predicate).ExecuteUpdateAsync(setPropertyExpression, cancellationToken).ConfigureAwait(false);

        _logger.LogBulkUpdateSuccess(updatedRows, VKTypeMetadataCache.GetName<TEntity>());

        return updatedRows;
    }

    private async Task<int> ExecuteDeleteInternalAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool forceDelete,
        CancellationToken cancellationToken)
    {
        VKGuard.NotNull(predicate);

        var query = GetQueryable(false).Where(predicate);

        if (!forceDelete && VKTypeMetadataCache.IsSoftDelete<TEntity>())
        {
            var propertySetter = new VKEFCorePropertySetter<TEntity>();

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
            var adapter = new EFCorePropertySetterAdapter<TEntity>(builder);
            setPropertyAction(adapter);

            // Bulk updates bypass the ChangeTracker, so Interceptors are NOT triggered.
            // We must manually invoke the processor to handle auditing fields (Validation is skipped).
            _processor.ProcessBulkUpdate(adapter);
        }, cancellationToken).ConfigureAwait(false);

        _logger.LogBulkUpdateSuccess(updatedRows, VKTypeMetadataCache.GetName<TEntity>());

        return updatedRows;
    }

    private async Task<int> ExecuteDeleteInternalAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool forceDelete,
        CancellationToken cancellationToken)
    {
        VKGuard.NotNull(predicate);

        var query = GetQueryable(false).Where(predicate);

        if (!forceDelete && VKTypeMetadataCache.IsSoftDelete<TEntity>())
        {
            var softDeletedRows = await query.ExecuteUpdateAsync(builder =>
            {
                var adapter = new EFCorePropertySetterAdapter<TEntity>(builder);

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

    /// <inheritdoc />
    public Task<int> ExecuteDeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return ExecuteDeleteInternalAsync(predicate, forceDelete: false, cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> ExecuteHardDeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return ExecuteDeleteInternalAsync(predicate, forceDelete: true, cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> ExecuteUpdateAsync(
        IVKSpecification<TEntity> specification,
        Action<IVKPropertySetter<TEntity>> setPropertyAction,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(specification);
        var predicate = specification.Criteria ?? (e => true);
        return ExecuteUpdateAsync(predicate, setPropertyAction, cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> ExecuteDeleteAsync(
        IVKSpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(specification);
        var predicate = specification.Criteria ?? (e => true);
        return ExecuteDeleteInternalAsync(predicate, forceDelete: false, cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> ExecuteHardDeleteAsync(
        IVKSpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(specification);
        var predicate = specification.Criteria ?? (e => true);
        return ExecuteDeleteInternalAsync(predicate, forceDelete: true, cancellationToken);
    }
}
