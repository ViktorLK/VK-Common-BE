using Microsoft.EntityFrameworkCore;

namespace VK.Blocks.Persistence.EFCore.Lifecycle.Internal;

/// <summary>
/// A default, no-op implementation of <see cref="IVKEntityLifecycleProcessor"/>
/// used when neither Auditing nor Soft Delete features are enabled.
/// This prevents dependency injection errors while adhering to non-nullable dependency rules.
/// </summary>
internal sealed class NoOpEntityLifecycleProcessor : IVKEntityLifecycleProcessor
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
    public void ProcessBulkUpdate<TEntity>(IVKPropertySetter<TEntity> setter) where TEntity : class
    {
        // No-op
    }

    /// <inheritdoc />
    public void ProcessBulkSoftDelete<TEntity>(IVKPropertySetter<TEntity> setter) where TEntity : class
    {
        // No-op
    }

}
