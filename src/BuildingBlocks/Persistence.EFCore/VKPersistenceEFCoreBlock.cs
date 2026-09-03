using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// A marker type for the VK.Blocks.Persistence.EFCore building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock), typeof(VKPersistenceBlock)])]
public sealed partial class VKPersistenceEFCoreBlock
{
    static partial void RegisterBlockCustom(IVKPersistenceEFCoreBuilder builder)
    {
        var services = builder.Services;

        // Core Model Creating Contributors
        services.TryAddEnumerable(Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton<IVKModelCreatingContributor, Database.Internal.ConcurrencyModelContributor>());
        services.TryAddEnumerable(Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton<IVKModelCreatingContributor, Database.Internal.ColumnOrderingModelContributor>());

        // Core Global Filter Contributors (Soft Delete)
        services.TryAddEnumerable(Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton<IVKGlobalFilterContributor, Database.Internal.SoftDeleteFilterContributor>());

        // DbContext Accessor (Scoped) [AP.02]
        services.TryAddScoped<IVKDbContextAccessor, Database.Internal.DefaultDbContextAccessor>();
        services.TryAddScoped(typeof(IVKDbContextAccessor<>), typeof(Database.Internal.DefaultDbContextAccessor<>));

        // Entity Lifecycle Processor (Scoped) [AP.02]
        services.TryAddScoped<IVKEntityLifecycleProcessor, Interceptors.Internal.DefaultEntityLifecycleProcessor>();

        var options = services.GetVKServiceInstance<VKPersistenceEFCoreOptions>();
        if (options is null)
            return;

        if (options.EnableAuditing)
        {
            services.TryAddScoped<VKAuditingInterceptor>();
        }

        if (options.EnableSoftDelete)
        {
            services.TryAddScoped<VKSoftDeleteInterceptor>();
        }

        if (options.EnableDomainEvents)
        {
            services.TryAddScoped<VKDomainEventsInterceptor>();
        }

        if (options.EnableOutbox)
        {
            services.TryAddEnumerable(Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton<IVKModelCreatingContributor, Database.Internal.OutboxModelContributor>());
            services.TryAddScoped<IVKOutboxStore, VKEFCoreOutboxStore>();
        }
    }
}
