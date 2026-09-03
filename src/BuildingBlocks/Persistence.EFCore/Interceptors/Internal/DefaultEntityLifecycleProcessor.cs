using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Interceptors.Internal;

/// <summary>
/// Default implementation of <see cref="IVKEntityLifecycleProcessor"/>.
/// </summary>
internal sealed class DefaultEntityLifecycleProcessor(IVKAuditProvider auditProvider) : IVKEntityLifecycleProcessor
{
    private readonly IVKAuditProvider _auditProvider = auditProvider;

    /// <inheritdoc />
    public void ProcessAuditing(DbContext context)
    {
        VKGuard.NotNull(context);

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            var type = entry.Entity.GetType();

            if (entry.State == EntityState.Added)
            {
                if (VKEntityMetadata.IsCreationAudited(type) && entry.Entity is IVKCreationAudited creationAudited)
                {
                    creationAudited.CreatedAt = _auditProvider.UtcNow;
                    creationAudited.CreatedBy = _auditProvider.CurrentUserId;
                }
            }
            else // Modified
            {
                if (VKEntityMetadata.IsModificationAudited(type) && entry.Entity is IVKModificationAudited modificationAudited)
                {
                    modificationAudited.UpdatedAt = _auditProvider.UtcNow;
                    modificationAudited.UpdatedBy = _auditProvider.CurrentUserId;
                }

                if (VKEntityMetadata.IsCreationAudited(type))
                {
                    entry.Property(nameof(IVKCreationAudited.CreatedAt)).IsModified = false;
                    entry.Property(nameof(IVKCreationAudited.CreatedBy)).IsModified = false;
                }
            }
        }
    }

    /// <inheritdoc />
    public void ProcessSoftDelete(DbContext context)
    {
        VKGuard.NotNull(context);

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            var type = entry.Entity.GetType();

            if (VKEntityMetadata.IsSoftDelete(type))
            {
                if (PhysicalDeleteRegistry.ShouldPhysicalDelete(entry.Entity))
                {
                    continue;
                }

                entry.State = EntityState.Modified;

                if (entry.Entity is IVKSoftDeletable softDeletable)
                {
                    softDeletable.IsDeleted = true;
                }

                if (VKEntityMetadata.IsDeletionAudited(type) && entry.Entity is IVKDeletionAudited deletionAudited)
                {
                    deletionAudited.DeletedAt = _auditProvider.UtcNow;
                    deletionAudited.DeletedBy = _auditProvider.CurrentUserId;
                }

                if (VKEntityMetadata.IsModificationAudited(type) && entry.Entity is IVKModificationAudited modificationAudited)
                {
                    modificationAudited.UpdatedAt = _auditProvider.UtcNow;
                    modificationAudited.UpdatedBy = _auditProvider.CurrentUserId;
                }
            }
        }
    }

    /// <inheritdoc />
    public void ProcessBulkUpdate<TEntity>(IVKPropertySetter<TEntity> setter) where TEntity : class
    {
        if (VKTypeMetadataCache.IsModificationAudited<TEntity>())
        {
            setter.SetProperty(e => ((IVKModificationAudited)e).UpdatedAt, _auditProvider.UtcNow);
            setter.SetProperty(e => ((IVKModificationAudited)e).UpdatedBy, _auditProvider.CurrentUserId);
        }
    }

    /// <inheritdoc />
    public void ProcessBulkSoftDelete<TEntity>(IVKPropertySetter<TEntity> setter) where TEntity : class
    {
        if (VKTypeMetadataCache.IsSoftDelete<TEntity>())
        {
            setter.SetProperty(e => ((IVKSoftDeletable)e).IsDeleted, true);

            if (VKTypeMetadataCache.IsDeletionAudited<TEntity>())
            {
                setter.SetProperty(e => ((IVKDeletionAudited)e).DeletedAt, _auditProvider.UtcNow);
                setter.SetProperty(e => ((IVKDeletionAudited)e).DeletedBy, _auditProvider.CurrentUserId);
            }

            if (VKTypeMetadataCache.IsModificationAudited<TEntity>())
            {
                setter.SetProperty(e => ((IVKModificationAudited)e).UpdatedAt, _auditProvider.UtcNow);
                setter.SetProperty(e => ((IVKModificationAudited)e).UpdatedBy, _auditProvider.CurrentUserId);
            }
        }
    }
}
