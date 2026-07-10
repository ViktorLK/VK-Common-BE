using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Interceptor to handle Soft Delete operations.
/// </summary>
public sealed class VKSoftDeleteInterceptor(
    IVKEntityLifecycleProcessor processor,
    ILogger<VKSoftDeleteInterceptor> logger) : SaveChangesInterceptor
{
    private readonly IVKEntityLifecycleProcessor _processor = VKGuard.NotNull(processor);
    private readonly ILogger<VKSoftDeleteInterceptor> _logger = VKGuard.NotNull(logger);

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is null)
        {
            throw new InvalidOperationException(
                $"{nameof(VKSoftDeleteInterceptor)}: {nameof(eventData.Context)} is null. " +
                "SaveChanges cannot proceed without a valid DbContext - soft-delete would be bypassed.");
        }

        _processor.ProcessSoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            throw new InvalidOperationException(
                $"{nameof(VKSoftDeleteInterceptor)}: {nameof(eventData.Context)} is null. " +
                "SaveChangesAsync cannot proceed without a valid DbContext - soft-delete would be bypassed.");
        }

        _processor.ProcessSoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
