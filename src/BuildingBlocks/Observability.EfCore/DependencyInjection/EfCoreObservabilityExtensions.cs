using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Observability.EfCore.Interceptors;
using VK.Blocks.Observability.EfCore.Options;

namespace VK.Blocks.Observability.EfCore.DependencyInjection;

/// <summary>
/// Extension methods for setting up EF Core Observability services in an <see cref="IServiceCollection" />.
/// </summary>
public static class EfCoreObservabilityExtensions
{
    /// <summary>
    /// Adds EF Core observability features including logging, tracing, and metrics interceptors.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configure">Optional configuration action for <see cref="EfCoreObservabilityOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddEfCoreObservability(
        this IServiceCollection services, 
        Action<EfCoreObservabilityOptions>? configure = null)
    {
        if (configure != null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton<QueryLoggingInterceptor>();
        services.TryAddSingleton<QueryTracingInterceptor>();
        services.TryAddSingleton<QueryMetricsInterceptor>();

        return services;
    }

    /// <summary>
    /// Adds observability interceptors to the <see cref="DbContextOptionsBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="DbContextOptionsBuilder"/>.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> used to resolve the interceptor singletons.</param>
    /// <returns>The updated <see cref="DbContextOptionsBuilder"/>.</returns>
    public static DbContextOptionsBuilder UseObservabilityInterceptors(
        this DbContextOptionsBuilder builder,
        IServiceProvider serviceProvider)
    {
        var loggingInterceptor = serviceProvider.GetService<QueryLoggingInterceptor>();
        if (loggingInterceptor != null)
        {
            builder.AddInterceptors(loggingInterceptor);
        }

        var tracingInterceptor = serviceProvider.GetService<QueryTracingInterceptor>();
        if (tracingInterceptor != null)
        {
            builder.AddInterceptors(tracingInterceptor);
        }

        var metricsInterceptor = serviceProvider.GetService<QueryMetricsInterceptor>();
        if (metricsInterceptor != null)
        {
            builder.AddInterceptors(metricsInterceptor);
        }

        return builder;
    }
}
