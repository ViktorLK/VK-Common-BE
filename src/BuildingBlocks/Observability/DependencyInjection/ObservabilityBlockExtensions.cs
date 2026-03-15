using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Observability.Enrichment;
using VK.Blocks.Observability.Options;

namespace VK.Blocks.Observability.DependencyInjection;

/// <summary>
/// Provides extension methods for dependency injection registration of the Observability block.
/// </summary>
public static class ObservabilityBlockExtensions
{
    #region Public Methods

    /// <summary>
    /// Registers Observability block services into the DI container and binds options from the provided configuration section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration section containing <see cref="ObservabilityOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddObservabilityBlock(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        return services.AddObservabilityBlock(options =>
        {
            configuration.Bind(options);
        });
    }

    /// <summary>
    /// Registers Observability block services into the DI container.
    /// <para>
    /// Components registered:
    /// <list type="bullet">
    ///   <item><description><see cref="ObservabilityOptions"/> — configured with DataAnnotation validation</description></item>
    ///   <item><description><see cref="ILogEnricher"/>s — including Application, UserContext, and TraceContext enrichers</description></item>
    ///   <item><description><see cref="ILogContextEnricher"/> — as <see cref="ActivityLogContextEnricher"/></description></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">The delegate to configure options.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddObservabilityBlock(
        this IServiceCollection services,
        Action<ObservabilityOptions> configure)
    {
        // Options — Enable DataAnnotation validation and validate on start
        services.AddOptions<ObservabilityOptions>()
            .Configure(configure)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Log Enrichers
        services.AddTransient<ILogEnricher, ApplicationEnricher>();
        services.AddTransient<ILogEnricher, UserContextEnricher>();
        services.AddTransient<ILogEnricher, TraceContextEnricher>();

        // Log Context Enricher (logging-provider neutral abstraction)
        services.AddTransient<ILogContextEnricher, ActivityLogContextEnricher>();

        return services;
    }

    #endregion
}
