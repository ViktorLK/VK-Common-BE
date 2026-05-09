using System;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Default implementation of <see cref="IVKEntityLifecycleProcessor"/>.
/// </summary>
public sealed class VKEntityLifecycleProcessor(IVKAuditProvider auditProvider) : IVKEntityLifecycleProcessor
{
    private readonly IVKAuditProvider _auditProvider = auditProvider;

    /// <inheritdoc />
    public void ProcessAuditing(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            // Optimization: Filter early by metadata. Note: entry.VKEntity casting is still required for property access.
            if (VKEntityMetadata.IsAuditable(entry.Entity.GetType()) && entry.Entity is IVKAuditable auditable)
            {
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedAt = _auditProvider.UtcNow;
                    auditable.CreatedBy = _auditProvider.CurrentUserId;
                }
                else // Modified
                {
                    auditable.UpdatedAt = _auditProvider.UtcNow;
                    auditable.UpdatedBy = _auditProvider.CurrentUserId;
                    entry.Property(nameof(IVKAuditable.CreatedAt)).IsModified = false;
                    entry.Property(nameof(IVKAuditable.CreatedBy)).IsModified = false;
                }
            }
        }
    }

    /// <inheritdoc />
    public void ProcessSoftDelete(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            if (VKEntityMetadata.IsSoftDelete(entry.Entity.GetType()) && entry.Entity is IVKSoftDelete softDelete)
            {
                entry.State = EntityState.Modified;

                softDelete.IsDeleted = true;
                softDelete.DeletedAt = _auditProvider.UtcNow;

                if (VKEntityMetadata.IsAuditable(entry.Entity.GetType()) && entry.Entity is IVKAuditable auditable)
                {
                    auditable.UpdatedAt = _auditProvider.UtcNow;
                    auditable.UpdatedBy = _auditProvider.CurrentUserId;
                }
            }
        }
    }

    /// <inheritdoc />
    public void ProcessBulkUpdate<TEntity>(IVKPropertySetter<TEntity> setter) where TEntity : class
    {
        if (VKTypeMetadataCache.IsAuditable<TEntity>())
        {
            setter.SetProperty(e => ((IVKAuditable)e).UpdatedAt, _auditProvider.UtcNow);
            setter.SetProperty(e => ((IVKAuditable)e).UpdatedBy, _auditProvider.CurrentUserId);
        }
    }

    /// <inheritdoc />
    public void ProcessBulkSoftDelete<TEntity>(IVKPropertySetter<TEntity> setter) where TEntity : class
    {
        if (VKTypeMetadataCache.IsSoftDelete<TEntity>())
        {
            setter.SetProperty(e => ((IVKSoftDelete)e).DeletedAt, _auditProvider.UtcNow);
            setter.SetProperty(e => ((IVKSoftDelete)e).IsDeleted, true);

            if (VKTypeMetadataCache.IsAuditable<TEntity>())
            {
                setter.SetProperty(e => ((IVKAuditable)e).UpdatedAt, _auditProvider.UtcNow);
                setter.SetProperty(e => ((IVKAuditable)e).UpdatedBy, _auditProvider.CurrentUserId);
            }
        }
    }
}
