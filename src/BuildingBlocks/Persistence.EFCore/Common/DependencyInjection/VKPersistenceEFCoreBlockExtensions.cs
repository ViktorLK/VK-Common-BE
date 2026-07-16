using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Persistence;
using VK.Blocks.Persistence.EFCore.Common.DependencyInjection.Internal;
using VK.Blocks.Persistence.EFCore.Interceptors.Internal;



namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Extension methods for setting up persistence services in an <see cref="IServiceCollection" />.
/// </summary>
public static partial class VKPersistenceEFCoreBlockExtensions
{
    public static IVKPersistenceEFCoreBuilder AddVKDbContext<TContext>(
        this IVKPersistenceEFCoreBuilder builder,
        Action<DbContextOptionsBuilder, VKDatabaseOptions>? dbContextOptions = null)
        where TContext : DbContext
    {
        var dbOptions = builder.Services.GetVKServiceInstance<VKDatabaseOptions>();
        if (dbOptions is null)
        {
            throw new InvalidOperationException("Database options must be registered before adding DbContext. Ensure the Database feature is registered by calling builder.AddVKDatabase().");
        }

        var defaultsOptions = VKGuard.NotNull(builder.Services.GetVKServiceInstance<VKPersistenceEFCoreOptions>());

        builder.Services.AddDbContext<TContext>((sp, dbBuilder) =>
        {
            if (dbOptions.UseNoTrackingByDefault)
            {
                dbBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }

            if (dbOptions.EnableSensitiveDataLogging)
            {
                dbBuilder.EnableSensitiveDataLogging();
            }

            if (dbOptions.EnableDetailedErrors)
            {
                dbBuilder.EnableDetailedErrors();
            }

            dbContextOptions?.Invoke(dbBuilder, dbOptions);

            var configurators = sp.GetServices<IVKDbContextOptionsConfigurator>();
            foreach (var configurator in configurators)
            {
                configurator.Configure(dbBuilder, sp);
            }

            ApplyFeatureInterceptors(dbBuilder, sp, defaultsOptions);
        });

        // Register lifecycle processors
        if (defaultsOptions.EnableAuditing == true || defaultsOptions.EnableSoftDelete == true || defaultsOptions.EnableMultiTenancy == true)
        {
            builder.Services.TryAddScoped<IVKEntityLifecycleProcessor, DefaultEntityLifecycleProcessor>();
        }
        builder.Services.TryAddScoped<IVKEntityLifecycleProcessor, NoOpEntityLifecycleProcessor>();

        // Register fallback audit provider if auditing is enabled
        var globalPersistenceOptions = builder.Services.GetVKServiceInstance<VKPersistenceOptions>();
        var enableAuditing = defaultsOptions.EnableAuditing ?? globalPersistenceOptions?.EnableAuditing ?? true;
        if (enableAuditing)
        {
            var currentDescriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IVKAuditProvider));
            if (currentDescriptor == null || currentDescriptor.ImplementationType?.FullName == "VK.Blocks.Persistence.Auditing.Internal.NoOpAuditProvider")
            {
                builder.Services.Replace(ServiceDescriptor.Scoped<IVKAuditProvider, BasicAuditProvider>());
            }
        }

        RegisterBasePersistenceComponents<TContext>(builder.Services);

        return builder;
    }

    public static IVKPersistenceEFCoreBuilder AddVKDbContext<TContext>(
        this IVKPersistenceEFCoreBuilder builder,
        Action<IVKDbContextBuilder<TContext>>? configure)
        where TContext : DbContext
    {
        VKGuard.NotNull(builder);

        // Call the main registration logic
        _ = builder.AddVKDbContext<TContext>((Action<DbContextOptionsBuilder, VKDatabaseOptions>?)null);

        // Execute local builder configuration
        var dbBuilder = new VKDbContextBuilder<TContext>(builder.Services, builder.Configuration);
        configure?.Invoke(dbBuilder);

        return builder;
    }

    private static void ApplyFeatureInterceptors(DbContextOptionsBuilder builder, IServiceProvider sp, VKPersistenceEFCoreOptions options)
    {
        if (options.EnableAuditing == true)
        {
            var auditInterceptor = sp.GetRequiredService<VKAuditingInterceptor>();
            builder.AddInterceptors(auditInterceptor);
        }

        if (options.EnableSoftDelete == true)
        {
            var softDeleteInterceptor = sp.GetRequiredService<VKSoftDeleteInterceptor>();
            builder.AddInterceptors(softDeleteInterceptor);
        }

        if (options.EnableMultiTenancy == true)
        {
            var tenantInterceptor = sp.GetRequiredService<VKTenantInterceptor>();
            builder.AddInterceptors(tenantInterceptor);
        }
    }

    private static void RegisterBasePersistenceComponents<TContext>(IServiceCollection services) where TContext : DbContext
    {
        services.TryAddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
        services.TryAddScoped<IVKUnitOfWork, VKUnitOfWork<TContext>>();
        services.TryAddScoped<IVKUnitOfWork<TContext>, VKUnitOfWork<TContext>>();
        services.TryAddScoped(typeof(IVKReadRepository<>), typeof(VKEFCoreReadRepository<>));
        services.TryAddScoped(typeof(IVKWriteRepository<>), typeof(VKEFCoreRepository<>));
        services.TryAddScoped(typeof(IVKBaseRepository<>), typeof(VKEFCoreRepository<>));
        services.TryAddScoped(typeof(IVKBulkRepository<>), typeof(VKEFCoreRepository<>));
    }
}
