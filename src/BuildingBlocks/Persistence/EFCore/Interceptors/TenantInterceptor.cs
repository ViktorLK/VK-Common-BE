using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VK.Blocks.Core.Primitives;
using VK.Blocks.MultiTenancy.Abstractions;
using VK.Blocks.MultiTenancy.Exceptions;

namespace VK.Blocks.Persistence.EFCore.Interceptors;

/// <summary>
/// Interceptor for injecting TenantId into IMultiTenant entities during save operations.
/// </summary>
public sealed class TenantInterceptor(ITenantProvider tenantProvider) : SaveChangesInterceptor
{
    private readonly ITenantProvider _tenantProvider = tenantProvider;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        InjectTenantId(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        InjectTenantId(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void InjectTenantId(DbContext? context)
    {
        if (context is null)
            return;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added && entry.Entity is IMultiTenant multiTenant)
            {
                if (string.IsNullOrWhiteSpace(multiTenant.TenantId))
                {
                    var tenantId = _tenantProvider.GetCurrentTenantId();
                    if (string.IsNullOrWhiteSpace(tenantId))
                    {
                        throw new TenantNotProvidedException($"Cannot save IMultiTenant entity of type '{entry.Entity.GetType().Name}': TenantId is missing from context.");
                    }
                    multiTenant.TenantId = tenantId;
                }
            }
        }
    }
}
