using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.MultiTenancy;
using VK.Blocks.Persistence.EFCore;

namespace VK.Blocks.MultiTenancy.EFCore;

/// <summary>
/// MultiTenancy.EFCore Building Block Marker.
/// Provides EF Core integration for multi-tenancy (Query Layer, Save Layer, Schema Layer, Runtime Layer).
/// Follows BB.02, AP.01, AP.02.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution and metadata; contains no business logic.")]
[VKBlockMarker(Dependencies = [typeof(VKMultiTenancyBlock), typeof(VKPersistenceEFCoreBlock)], Toggleable = false)]
public sealed partial class VKMultiTenancyEFCoreBlock
{
    static partial void RegisterBlockCustom(IVKMultiTenancyEFCoreBuilder builder)
    {
        var services = builder.Services;

        // 1. Query Layer (Query Filter Contributor)
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IVKGlobalFilterContributor, VKMultiTenantQueryFilterContributor>());

        // 2. Schema Layer (Physical Constraints Contributor)
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IVKModelCreatingContributor, VKMultiTenantConstraintModelContributor>());

        // 3. Save Layer (Saving/Connection Interceptor)
        services.TryAddScoped<VKTenantInterceptor>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor, VKTenantInterceptor>(sp => sp.GetRequiredService<VKTenantInterceptor>()));

        // 4. Runtime Layer (Options Configurator for Database-per-tenant)
        services.TryAddTransient<IVKDbContextOptionsConfigurator, VKMultiTenantDbContextOptionsConfigurator>();
    }
}
