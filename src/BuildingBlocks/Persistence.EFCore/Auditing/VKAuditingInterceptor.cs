using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Interceptor to automatically update auditable entities.
/// </summary>
public sealed class VKAuditingInterceptor(
    IVKEntityLifecycleProcessor processor,
    ILogger<VKAuditingInterceptor> logger) : SaveChangesInterceptor
{
    private readonly IVKEntityLifecycleProcessor _processor = VKGuard.NotNull(processor);
    private readonly ILogger<VKAuditingInterceptor> _logger = VKGuard.NotNull(logger);

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        VKGuard.NotNull(eventData);

        if (eventData.Context is null)
        {
            throw new InvalidOperationException("DbContext is null in VKAuditingInterceptor.");
        }

        _processor.ProcessAuditing(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(eventData);

        if (eventData.Context is null)
        {
            throw new InvalidOperationException("DbContext is null in VKAuditingInterceptor.");
        }

        _processor.ProcessAuditing(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
