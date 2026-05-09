using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore.Auditing.Internal;
using VK.Blocks.Persistence.EFCore.Lifecycle.Internal;
using VK.Blocks.Persistence.EFCore.MultiTenancy.Internal;
using VK.Blocks.Persistence.EFCore.Pagination.Internal;

namespace VK.Blocks.Persistence.EFCore.DependencyInjection.Internal;

internal static class PersistenceEFCoreBlockRegistration
{
    public static IVKPersistenceEFCoreBuilder Register<TContext>(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKPersistenceEFCoreOptions, VKPersistenceEFCoreOptions>? transformOptions = null,
        Action<DbContextOptionsBuilder, VKPersistenceEFCoreOptions>? dbContextOptions = null)
        where TContext : DbContext
    {
        var builder = new PersistenceEFCoreBlockBuilder(services, configuration);

        // 1. Check-Self & Prerequisite
        if (services.IsVKBlockRegistered<VKPersistenceEFCoreBlock>())
        {
            return builder;
        }
        services.EnsureCoreBlockRegistered<VKPersistenceEFCoreBlock>();

        // 2. Options Registration
        var baseOptions = services.AddVKBlockOptions<VKPersistenceOptions>(configuration);
        var options = services.AddVKBlockOptions<VKPersistenceEFCoreOptions>(configuration, transformOptions);

        // Merge base options into EF Core options if not explicitly set
        options = options with
        {
            EnableAuditing = options.EnableAuditing ?? baseOptions.EnableAuditing,
            EnableSoftDelete = options.EnableSoftDelete ?? baseOptions.EnableSoftDelete,
            EnableMultiTenancy = options.EnableMultiTenancy ?? baseOptions.EnableMultiTenancy
        };

        // 3. Mark-Self
        services.AddVKBlockMarker<VKPersistenceEFCoreBlock>();

        // 4. Options Validation
        services.TryAddEnumerableSingleton<IValidateOptions<VKPersistenceEFCoreOptions>, VKPersistenceEFCoreOptionsValidator>();

        // 5. Diagnostics / Static Metadata
        // Managed by [VKBlockDiagnostics] if marker is annotated. 

        // 6. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 7. Core Services
        RegisterFeatureServices(services, options);

        services.AddDbContext<TContext>((sp, builder) =>
        {
            // A. Apply standard configuration (CS.04, OR.02)
            if (options.UseNoTrackingByDefault)
            {
                builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }

            if (options.EnableSensitiveDataLogging)
            {
                builder.EnableSensitiveDataLogging();
            }

            if (options.EnableDetailedErrors)
            {
                builder.EnableDetailedErrors();
            }

            // B. Apply static configuration (Provider, etc.)
            dbContextOptions?.Invoke(builder, options);

            // C. Apply dynamic configuration via Configurators
            var configurators = sp.GetServices<IVKDbContextOptionsConfigurator>();
            foreach (var configurator in configurators)
            {
                configurator.Configure(builder, sp);
            }

            // D. Apply standard feature interceptors (Audit, Soft-Delete, TenantId injection)
            ApplyFeatureInterceptors(builder, sp, options);
        });

        RegisterBasePersistenceComponents<TContext>(services);

        return builder;
    }

    private static void RegisterFeatureServices(IServiceCollection services, VKPersistenceEFCoreOptions options)
    {
        if (options.EnableAuditing == true || options.EnableSoftDelete == true)
        {
            services.TryAddScoped<IVKEntityLifecycleProcessor, VKEntityLifecycleProcessor>();
        }

        if (options.EnableMultiTenancy == true)
        {
            services.TryAddScoped<VKTenantInterceptor>();
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKDbContextOptionsConfigurator, MultiTenantDbContextOptionsConfigurator>());
        }

        if (options.EnableAuditing == true)
        {
            services.TryAddScoped<VKAuditingInterceptor>();
            services.TryAddScoped<IVKAuditProvider, AuditProvider>();
        }

        if (options.EnableSoftDelete == true)
        {
            services.TryAddScoped<VKSoftDeleteInterceptor>();
        }

        // Always ensure IVKEntityLifecycleProcessor is registered as a fallback.
        services.TryAddScoped<IVKEntityLifecycleProcessor, NoOpEntityLifecycleProcessor>();
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
        services.TryAddScoped(typeof(IVKReadRepository<>), typeof(VKEfCoreReadRepository<>));
        services.TryAddScoped(typeof(IVKWriteRepository<>), typeof(VKEfCoreRepository<>));
        services.TryAddScoped(typeof(IVKBaseRepository<>), typeof(VKEfCoreRepository<>));

        // Register SimpleCursorSerializer as the default.
        services.TryAddSingleton<IVKCursorSerializer, SimpleCursorSerializer>();
    }
}

