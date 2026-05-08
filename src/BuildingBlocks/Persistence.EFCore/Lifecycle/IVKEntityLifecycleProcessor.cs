using Microsoft.EntityFrameworkCore;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Service for processing entity lifecycle events such as auditing and soft delete.
/// </summary>
public interface IVKEntityLifecycleProcessor
{

    /// <summary>
    /// Processes auditing fields (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) for entities in the ChangeTracker.
    /// </summary>
    /// <param name="context">The DbContext.</param>
    void ProcessAuditing(DbContext context);

    /// <summary>
    /// Processes soft delete logic for entities marked as Deleted in the ChangeTracker.
    /// </summary>
    /// <param name="context">The DbContext.</param>
    void ProcessSoftDelete(DbContext context);

    /// <summary>
    /// Processes auditing fields for bulk update operations.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="setter">The property setter.</param>
    void ProcessBulkUpdate<TEntity>(IVKPropertySetter<TEntity> setter) where TEntity : class;

    /// <summary>
    /// Processes soft delete fields for bulk delete operations.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="setter">The property setter.</param>
    void ProcessBulkSoftDelete<TEntity>(IVKPropertySetter<TEntity> setter) where TEntity : class;

}
