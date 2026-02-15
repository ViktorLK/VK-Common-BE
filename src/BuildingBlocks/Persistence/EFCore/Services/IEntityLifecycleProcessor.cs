using Microsoft.EntityFrameworkCore;
using VK.Blocks.Persistence.Abstractions.Repositories;

namespace VK.Blocks.Persistence.EFCore.Services;

/// <summary>
/// Service for processing entity lifecycle events such as auditing and soft delete.
/// </summary>
public interface IEntityLifecycleProcessor
{
    #region Public Methods

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
    void ProcessBulkUpdate<TEntity>(IPropertySetter<TEntity> setter) where TEntity : class;

    /// <summary>
    /// Processes soft delete fields for bulk delete operations.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="setter">The property setter.</param>
    void ProcessBulkSoftDelete<TEntity>(IPropertySetter<TEntity> setter) where TEntity : class;

    #endregion
}
