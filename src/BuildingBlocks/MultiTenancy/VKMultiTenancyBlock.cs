using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.MultiTenancy.Context.Internal;
using VK.Blocks.MultiTenancy.Entitlements.Internal;
using VK.Blocks.MultiTenancy.Tenants.Internal;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// A marker type for the VK.Blocks.MultiTenancy building block.
/// Follows BB.02, AP.01, AP.02.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKMultiTenancyBlock
{
    static partial void RegisterBlockCustom(IVKMultiTenancyBuilder builder)
    {
        var services = builder.Services;

        // Factory & Stores
        services.TryAddSingleton<InMemoryTenantStore>();
        services.TryAddSingleton<IVKTenantStore>(sp => sp.GetRequiredService<InMemoryTenantStore>());
        services.TryAddSingleton<IVKTenantCacheInvalidator>(sp => sp.GetRequiredService<InMemoryTenantStore>());

        // Rich Tenant Context (Supports Scoped + Ambient AsyncLocal)
        services.TryAddScoped<TenantContextAccessor>();
        services.TryAddScoped<IVKTenantContext>(sp => sp.GetRequiredService<TenantContextAccessor>());
        
        // AP.06 / Dual Registration: Replace Core defaults with MultiTenancy active providers
        services.Replace(ServiceDescriptor.Scoped<IVKTenantCoordinate>(sp => sp.GetRequiredService<TenantContextAccessor>()));
        services.TryAddScoped<TenantContextTenantProvider>();
        services.Replace(ServiceDescriptor.Scoped<IVKTenantProvider>(sp => sp.GetRequiredService<TenantContextTenantProvider>()));
        services.Replace(ServiceDescriptor.Scoped<IVKActiveTenantProvider, MultiTenancyActiveTenantProvider>());

        // Evaluators (Core Entitlements)
        services.TryAddScoped<IVKTenantFeatureEvaluator, TenantFeatureEvaluator>();
    }
}
