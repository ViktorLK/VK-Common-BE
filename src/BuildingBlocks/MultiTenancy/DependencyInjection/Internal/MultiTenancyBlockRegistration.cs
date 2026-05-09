using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.MultiTenancy.Context.Internal;
using VK.Blocks.MultiTenancy.Entitlements.Internal;
using VK.Blocks.MultiTenancy.Internal;

namespace VK.Blocks.MultiTenancy.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the MultiTenancy core block.
/// </summary>
internal static class MultiTenancyBlockRegistration
{
    public static IVKMultiTenancyBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKMultiTenancyOptions, VKMultiTenancyOptions>? transform = null)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);

        // 1. Check-Self & Prerequisite
        if (services.IsVKBlockRegistered<VKMultiTenancyBlock>())
        {
            return new VKMultiTenancyBuilder(services, configuration);
        }

        // 2. Options Registration
        var options = services.AddVKBlockOptions<VKMultiTenancyOptions>(configuration, transform);

        // 3. Mark-Self
        services.AddVKBlockMarker<VKMultiTenancyBlock>();

        // 5. Diagnostics
        // BB.04: Use static partial class [VKBlockDiagnostics] for telemetry.
        // Static diagnostics do not require DI registration.

        // 6. Feature Toggle
        if (!options.Enabled)
        {
            return new VKMultiTenancyBuilder(services, configuration);
        }

        // 4. Core Services
        RegisterCoreServices(services, options);

        return new VKMultiTenancyBuilder(services, configuration);
    }

    private static void RegisterCoreServices(IServiceCollection services, VKMultiTenancyOptions options)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(options);

        services.TryAddSingleton<IVKTenantInfoFactory, TenantInfoFactory>();

        // Context & Accessors
        services.TryAddScoped<TenantContext>();
        services.TryAddScoped<IVKTenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.TryAddScoped<IVKTenantContextSetter>(sp => sp.GetRequiredService<TenantContext>());
        services.TryAddScoped<IVKTenantProvider, TenantContextTenantProvider>();
        services.TryAddScoped<TenantContextAccessor>();

        // Evaluators (Core Entitlements)
        services.TryAddScoped<IVKTenantFeatureEvaluator, TenantFeatureEvaluator>();
    }
}

