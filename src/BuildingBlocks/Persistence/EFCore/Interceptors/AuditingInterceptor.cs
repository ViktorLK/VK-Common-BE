using Microsoft.EntityFrameworkCore.Diagnostics;
using VK.Blocks.Persistence.Abstractions.Entities;
using VK.Blocks.Persistence.EFCore.Services;

namespace VK.Blocks.Persistence.EFCore.Interceptors;

/// <summary>
/// Interceptor to automatically update <see cref="IAuditable"/> entities.
/// </summary>
public class AuditingInterceptor(IEntityLifecycleProcessor processor) : SaveChangesInterceptor
{
    #region Fields

    private readonly IEntityLifecycleProcessor _processor = processor;

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        _processor.ProcessAuditing(eventData.Context!);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        _processor.ProcessAuditing(eventData.Context!);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    #endregion
}
