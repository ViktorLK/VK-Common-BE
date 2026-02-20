using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using VK.Blocks.Web.CorrelationId;
using VK.Blocks.Web.Infrastructure;

namespace VK.Blocks.Web.Extensions;

/// <summary>
/// Service collection extensions for configuring API standards.
/// </summary>
public static class ServiceCollectionExtensions
{
    #region Public Methods

    /// <summary>
    /// Adds API standards services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    ///
    public static IServiceCollection AddApiStandards(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();



        return services;
    }

    public static IServiceCollection AddCorrelationId(this IServiceCollection services, IConfiguration configuration)
    {

        services.Configure<CorrelationIdOptions>(configuration.GetSection("CorrelationId"));
        services.AddScoped<ICorrelationIdProvider, DefaultCorrelationIdProvider>();

        return services;
    }

    public static IServiceCollection AddCorrelationId(this IServiceCollection services, Action<CorrelationIdOptions> correlationIdOptions)
    {
        services.Configure(correlationIdOptions);
        services.AddScoped<ICorrelationIdProvider, DefaultCorrelationIdProvider>();

        return services;
    }

    public static IApplicationBuilder UseApiStandards(this IApplicationBuilder app)
    {
        app.UseCorrelationId();
        app.UseExceptionHandler();

        return app;
    }

    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }

    #endregion
}
