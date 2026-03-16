using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using VK.Blocks.Observability.Serilog.Enrichers;
using VK.Blocks.Observability.Serilog.Options;
using VK.Blocks.Observability.Serilog.Sinks;

namespace VK.Blocks.Observability.Serilog.DependencyInjection;

/// <summary>
/// Extension methods for registering Serilog observability services.
/// </summary>
public static class SerilogObservabilityExtensions
{
    /// <summary>
    /// Adds the VK Serilog observability block to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    public static IServiceCollection AddVKSerilogBlock(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SerilogOptions>(configuration.GetSection(SerilogOptions.SectionName));

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
        });

        services.AddSerilog((servicesProvider, loggerConfiguration) =>
        {
            var options = servicesProvider.GetRequiredService<IOptions<SerilogOptions>>().Value;

            if (!options.Enabled)
                return;

            loggerConfiguration
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext();

            // Register Enrichers
            if (options.EnableTraceContext)
            {
                loggerConfiguration.Enrich.With(new TraceContextEnricher());
            }

            if (options.EnableApplicationEnricher)
            {
                var env = servicesProvider.GetRequiredService<IHostEnvironment>();
                loggerConfiguration.Enrich.With(new ApplicationEnricher(env));
            }

            if (options.EnableUserContext)
            {
                var httpContextAccessor = servicesProvider.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
                if (httpContextAccessor != null)
                {
                    loggerConfiguration.Enrich.With(new UserContextEnricher(httpContextAccessor));
                }
            }

            if (options.SensitiveKeywords.Count != 0)
            {
                loggerConfiguration.Enrich.With(new SensitiveDataEnricher(options.SensitiveKeywords));
            }

            // Configure Sinks
            ConsoleSinkConfigurator.Configure(loggerConfiguration, options.Console);
            FileSinkConfigurator.Configure(loggerConfiguration, options.File);
        });

        return services;
    }
}
