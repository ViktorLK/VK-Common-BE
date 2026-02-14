using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Persistence.Abstractions;
using VK.Blocks.Persistence.Abstractions.Auditing;
using VK.Blocks.Persistence.Abstractions.Options;
using VK.Blocks.Persistence.EFCore.Interceptors;
using VK.Blocks.Persistence.EFCore.Services;

namespace VK.Blocks.Persistence.EFCore.DependencyInjection;

/// <summary>
/// Extension methods for setting up persistence services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    #region Public Methods

    /// <summary>
    /// Adds and configures the DbContext and related services.
    /// </summary>
    /// <typeparam name="TContext">The type of the DbContext.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure persistence options.</param>
    /// <param name="dbContextOptions">Action to configure DbContext options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddVKDbContext<TContext>(
        this IServiceCollection services,
        Action<PersistenceOptions> configureOptions,
        Action<DbContextOptionsBuilder> dbContextOptions)
        where TContext : DbContext
    {
        var options = new PersistenceOptions();
        configureOptions(options);
        services.AddSingleton(options);

        if (options.EnableAuditing || options.EnableSoftDelete)
        {
            services.TryAddScoped<IEntityLifecycleProcessor, EntityLifecycleProcessor>();
        }

        if (options.EnableAuditing)
        {
            services.AddScoped<AuditingInterceptor>();
            services.TryAddScoped<IAuditProvider, DefaultAuditProvider>();
        }

        if (options.EnableSoftDelete)
        {
            services.AddScoped<SoftDeleteInterceptor>();
        }

        services.AddDbContext<TContext>((sp, builder) =>
        {
            dbContextOptions(builder);

            if (options.EnableAuditing)
            {
                var auditInterceptor = sp.GetRequiredService<AuditingInterceptor>();
                builder.AddInterceptors(auditInterceptor);
            }

            if (options.EnableSoftDelete)
            {
                var softDeleteInterceptor = sp.GetRequiredService<SoftDeleteInterceptor>();
                builder.AddInterceptors(softDeleteInterceptor);
            }
        });

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();

        return services;
    }

    #endregion
}
