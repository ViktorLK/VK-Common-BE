using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy.Tenants.Internal;

/// <summary>
/// Default in-memory implementation of <see cref="IVKTenantStore"/> and <see cref="IVKTenantCacheInvalidator"/>.
/// Ideal for local development, testing, or static single/multi-tenant definitions.
/// Follows AP.01, AP.03, CS.01.
/// </summary>
internal sealed class InMemoryTenantStore : IVKTenantStore, IVKTenantCacheInvalidator
{
    private readonly ConcurrentDictionary<VKTenantId, VKTenantInfo> _tenantsById = new();
    private readonly ConcurrentDictionary<string, VKTenantId> _tenantIdByDomain = new(System.StringComparer.OrdinalIgnoreCase);

    public InMemoryTenantStore()
    {
        // Seed default system tenant
        var defaultTenant = VKTenantInfo.Create(VKTenantId.Default, "Default Tenant");
        AddOrUpdate(defaultTenant);
    }

    public InMemoryTenantStore(IEnumerable<VKTenantInfo> initialTenants)
    {
        foreach (var tenant in initialTenants)
        {
            AddOrUpdate(tenant);
        }
    }

    public void AddOrUpdate(VKTenantInfo tenant)
    {
        VKGuard.NotNull(tenant);
        _tenantsById[tenant.Id] = tenant;
        if (!string.IsNullOrWhiteSpace(tenant.Domain))
        {
            _tenantIdByDomain[tenant.Domain] = tenant.Id;
        }
    }

    public Task<VKResult<VKTenantInfo>> GetByIdAsync(VKTenantId tenantId, CancellationToken cancellationToken = default)
    {
        if (_tenantsById.TryGetValue(tenantId, out var tenant))
        {
            return Task.FromResult(VKResult.Success(tenant));
        }

        return Task.FromResult(VKResult.Failure<VKTenantInfo>(VKMultiTenancyErrors.TenantNotFound));
    }

    public Task<VKResult<VKTenantInfo>> GetByDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(domain) &&
            _tenantIdByDomain.TryGetValue(domain, out var tenantId) &&
            _tenantsById.TryGetValue(tenantId, out var tenant))
        {
            return Task.FromResult(VKResult.Success(tenant));
        }

        return Task.FromResult(VKResult.Failure<VKTenantInfo>(VKMultiTenancyErrors.TenantNotFound));
    }

    public Task<VKResult<IReadOnlyList<VKTenantInfo>>> GetActiveTenantsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<VKTenantInfo> activeTenants = _tenantsById.Values.Where(t => t.IsActive).ToList();
        return Task.FromResult(VKResult.Success(activeTenants));
    }

    public Task<VKResult> InvalidateAsync(VKTenantId tenantId, CancellationToken cancellationToken = default)
    {
        _tenantsById.TryRemove(tenantId, out var removed);
        if (removed?.Domain is not null)
        {
            _tenantIdByDomain.TryRemove(removed.Domain, out _);
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> InvalidateAllAsync(CancellationToken cancellationToken = default)
    {
        _tenantsById.Clear();
        _tenantIdByDomain.Clear();
        return Task.FromResult(VKResult.Success());
    }
}
