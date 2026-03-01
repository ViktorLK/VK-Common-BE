using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core.DependencyInjection;
using VK.Blocks.Persistence.Abstractions;
using VK.Blocks.Persistence.Abstractions.Auditing;
using VK.Blocks.Persistence.Abstractions.Options;
using VK.Blocks.Persistence.Abstractions.Repositories;
using VK.Blocks.Persistence.Core.Pagination;
using VK.Blocks.Persistence.EFCore.Auditing;
using VK.Blocks.Persistence.EFCore.Infrastructure;
using VK.Blocks.Persistence.EFCore.Interceptors;
using VK.Blocks.Persistence.EFCore.Options;
using VK.Blocks.Persistence.EFCore.Repositories;
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
        var options = services.AddVKBlockOptions(configureOptions);

        services.RegisterFeatureServices(options);

        services.AddDbContext<TContext>((sp, builder) =>
        {
            dbContextOptions(builder);
            builder.AddFeatureInterceptors(sp, options);
        });

        services.RegisterBasePersistenceComponents<TContext>();
        services.WarnIfInsecureCursor();

        return services;
    }

    /// <summary>
    /// Overrides the default cursor serializer with <see cref="SecureCursorSerializer"/>,
    /// which uses HMAC-SHA256 signing, schema versioning, and optional token expiry.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">An action to configure <see cref="CursorSerializerOptions"/>.</param>
    /// <returns>The service collection.</returns>
    /// <example>
    /// <code>
    /// services.AddSecureCursorSerializer(opts =>
    /// {
    ///     opts.SigningKey = builder.Configuration["CursorSerializer:SigningKey"];
    ///     opts.DefaultExpiry = TimeSpan.FromHours(1);
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddSecureCursorSerializer(
        this IServiceCollection services,
        Action<CursorSerializerOptions> configureOptions)
    {
        // Replace the default SimpleCursorSerializer with the secure implementation.
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICursorSerializer));
        if (descriptor is not null)
        {
            services.Remove(descriptor);
        }

        services.AddSingleton<ICursorSerializer, SecureCursorSerializer>();

        services.AddOptions<CursorSerializerOptions>()
            .Configure(configureOptions)
            .Validate(o => !string.IsNullOrWhiteSpace(o.SigningKey), "SigningKey must not be empty.")
            .ValidateOnStart();

        return services;
    }

    #endregion

    #region Private Methods

    private static IServiceCollection RegisterFeatureServices(this IServiceCollection services, PersistenceOptions options)
    {
        if (options.EnableAuditing || options.EnableSoftDelete)
        {
            services.TryAddScoped<IEntityLifecycleProcessor, EntityLifecycleProcessor>();
        }

        if (options.EnableMultiTenancy)
        {
            services.AddScoped<TenantInterceptor>();
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

        // Always ensure IEntityLifecycleProcessor is registered so that EfCoreRepository can depend on it without being nullable.
        // It will be overridden by EntityLifecycleProcessor if Auditing or SoftDelete is enabled (registered above).
        services.TryAddScoped<IEntityLifecycleProcessor, NoOpEntityLifecycleProcessor>();

        return services;
    }

    private static DbContextOptionsBuilder AddFeatureInterceptors(
        this DbContextOptionsBuilder builder,
        IServiceProvider sp,
        PersistenceOptions options)
    {
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

        if (options.EnableMultiTenancy)
        {
            var tenantInterceptor = sp.GetRequiredService<TenantInterceptor>();
            builder.AddInterceptors(tenantInterceptor);
        }

        return builder;
    }

    private static IServiceCollection RegisterBasePersistenceComponents<TContext>(this IServiceCollection services) where TContext : DbContext
    {
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
        services.TryAddScoped(typeof(IReadRepository<>), typeof(EfCoreReadRepository<>));
        services.TryAddScoped(typeof(IWriteRepository<>), typeof(EfCoreRepository<>));
        services.TryAddScoped(typeof(IBaseRepository<>), typeof(EfCoreRepository<>));

        // Register SimpleCursorSerializer as the default. Call AddSecureCursorSerializer() to override.
        services.TryAddSingleton<ICursorSerializer, SimpleCursorSerializer>();

        return services;
    }

    private static IServiceCollection WarnIfInsecureCursor(this IServiceCollection services)
    {
        services.AddOptions<CursorSerializerOptions>()
            .PostConfigure<IServiceProvider>((opts, sp) =>
            {
                var env = sp.GetService<IHostEnvironment>();
                if (env?.IsProduction() == true)
                {
                    var serializer = sp.GetService<ICursorSerializer>();
                    if (serializer is SimpleCursorSerializer)
                    {
                        var logger = sp.GetService<ILogger<ICursorSerializer>>();
                        logger?.LogWarning(
                            "SECURITY WARNING: SimpleCursorSerializer is currently registered as the ICursorSerializer in a Production environment. " +
                            "This serializer is not secure against tampering. Consider using AddSecureCursorSerializer() to enable HMAC-SHA256 signatures for pagination cursors.");
                    }
                }
            });

        return services;
    }

    #endregion
}
