using Microsoft.EntityFrameworkCore.Diagnostics;
using VK.Blocks.Persistence.EFCore.Services;

namespace VK.Blocks.Persistence.EFCore.Interceptors;

/// <summary>
/// Interceptor to handle Soft Delete operations.
/// </summary>
public class SoftDeleteInterceptor(IEntityLifecycleProcessor processor) : SaveChangesInterceptor
{
    #region Fields

    private readonly IEntityLifecycleProcessor _processor = processor;

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        // A null Context here indicates an EF Core misconfiguration or internal bug.
        // We must NOT proceed — skipping soft-delete processing would cause a physical DELETE
        // to execute instead of the expected logical soft-delete, permanently destroying data.
        if (eventData.Context is null)
        {
            throw new InvalidOperationException(
                $"{nameof(SoftDeleteInterceptor)}: {nameof(eventData.Context)} is null. " +
                "SaveChanges cannot proceed without a valid DbContext — soft-delete would be bypassed.");
        }

        _processor.ProcessSoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        // A null Context here indicates an EF Core misconfiguration or internal bug.
        // We must NOT proceed — skipping soft-delete processing would cause a physical DELETE
        // to execute instead of the expected logical soft-delete, permanently destroying data.
        if (eventData.Context is null)
        {
            throw new InvalidOperationException(
                $"{nameof(SoftDeleteInterceptor)}: {nameof(eventData.Context)} is null. " +
                "SaveChangesAsync cannot proceed without a valid DbContext — soft-delete would be bypassed.");
        }

        _processor.ProcessSoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    #endregion
}
