using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

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
        var dbOptions = builder.Services.GetVKServiceInstance<VKDatabaseOptions>()!;
        var defaultsOptions = builder.Services.GetVKServiceInstance<VKPersistenceEFCoreOptions>()!;

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

        RegisterBasePersistenceComponents<TContext>(builder.Services);

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
    }
}


