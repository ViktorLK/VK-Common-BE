using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.MultiTenancy.Context.Internal;
using VK.Blocks.MultiTenancy.Entitlements.Internal;
using VK.Blocks.MultiTenancy.Internal;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// A marker type for the VK.Blocks.MultiTenancy building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKMultiTenancyBlock
{
    static partial void RegisterBlockCustom(IVKMultiTenancyBuilder builder)
    {
        var services = builder.Services;

        services.TryAddSingleton<IVKTenantInfoFactory, TenantInfoFactory>();

        // Context & Accessors
        services.TryAddScoped<TenantContext>();
        services.TryAddScoped<IVKTenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.TryAddScoped<IVKTenantContextSetter>(sp => sp.GetRequiredService<TenantContext>());
        services.TryAddScoped<IVKTenantProvider, TenantContextTenantProvider>();
        services.TryAddScoped<IVKActiveTenantProvider, MultiTenancyActiveTenantProvider>();
        services.TryAddScoped<TenantContextAccessor>();

        // Evaluators (Core Entitlements)
        services.TryAddScoped<IVKTenantFeatureEvaluator, TenantFeatureEvaluator>();
    }
}
