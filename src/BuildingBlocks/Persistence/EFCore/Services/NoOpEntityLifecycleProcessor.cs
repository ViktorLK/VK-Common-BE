using Microsoft.EntityFrameworkCore;
using VK.Blocks.Persistence.Abstractions.Repositories;

namespace VK.Blocks.Persistence.EFCore.Services;

/// <summary>
/// A default, no-op implementation of <see cref="IEntityLifecycleProcessor"/>
/// used when neither Auditing nor Soft Delete features are enabled.
/// This prevents dependency injection errors while adhering to non-nullable dependency rules.
/// </summary>
public sealed class NoOpEntityLifecycleProcessor : IEntityLifecycleProcessor
{
    /// <inheritdoc />
    public void ProcessAuditing(DbContext context)
    {
        // No-op
    }

    /// <inheritdoc />
    public void ProcessSoftDelete(DbContext context)
    {
        // No-op
    }

    /// <inheritdoc />
    public void ProcessBulkUpdate<TEntity>(IPropertySetter<TEntity> setter) where TEntity : class
    {
        // No-op
    }

    /// <inheritdoc />
    public void ProcessBulkSoftDelete<TEntity>(IPropertySetter<TEntity> setter) where TEntity : class
    {
        // No-op
    }
}
