using Microsoft.EntityFrameworkCore;
using VK.Blocks.Persistence.Abstractions.Auditing;
using VK.Blocks.Core.Primitives;
using VK.Blocks.Persistence.Abstractions.Repositories;
using VK.Blocks.Persistence.EFCore.Caches;

namespace VK.Blocks.Persistence.EFCore.Services;

/// <summary>
/// Default implementation of <see cref="IEntityLifecycleProcessor"/>.
/// </summary>
public class EntityLifecycleProcessor(IAuditProvider auditProvider) : IEntityLifecycleProcessor
{
    #region Fields

    private readonly IAuditProvider _auditProvider = auditProvider;

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public void ProcessAuditing(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditable auditable)
            {
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedAt = _auditProvider.UtcNow;
                    auditable.CreatedBy = _auditProvider.CurrentUserId;
                }

                if (entry.State == EntityState.Modified)
                {
                    auditable.UpdatedAt = _auditProvider.UtcNow;
                    auditable.UpdatedBy = _auditProvider.CurrentUserId;
                    entry.Property(nameof(IAuditable.CreatedAt)).IsModified = false;
                    entry.Property(nameof(IAuditable.CreatedBy)).IsModified = false;
                }
            }
        }
    }

    /// <inheritdoc />
    public void ProcessSoftDelete(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var entry in context.ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;

                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = _auditProvider.UtcNow;

                if (entry.Entity is IAuditable auditable)
                {
                    auditable.UpdatedAt = _auditProvider.UtcNow;
                    auditable.UpdatedBy = _auditProvider.CurrentUserId;
                }
            }
        }
    }

    /// <inheritdoc />
    public void ProcessBulkUpdate<TEntity>(IPropertySetter<TEntity> setter) where TEntity : class
    {
        if (EfCoreTypeCache<TEntity>.IsAuditable)
        {
            setter.SetProperty(e => ((IAuditable)e).UpdatedAt, _auditProvider.UtcNow);
            setter.SetProperty(e => ((IAuditable)e).UpdatedBy, _auditProvider.CurrentUserId);
        }
    }

    /// <inheritdoc />
    public void ProcessBulkSoftDelete<TEntity>(IPropertySetter<TEntity> setter) where TEntity : class
    {
        if (EfCoreTypeCache<TEntity>.IsSoftDelete)
        {
            setter.SetProperty(e => ((ISoftDelete)e).DeletedAt, _auditProvider.UtcNow);
            setter.SetProperty(e => ((ISoftDelete)e).IsDeleted, true);

            if (EfCoreTypeCache<TEntity>.IsAuditable)
            {
                setter.SetProperty(e => ((IAuditable)e).UpdatedAt, _auditProvider.UtcNow);
                setter.SetProperty(e => ((IAuditable)e).UpdatedBy, _auditProvider.CurrentUserId);
            }
        }
    }

    #endregion
}
