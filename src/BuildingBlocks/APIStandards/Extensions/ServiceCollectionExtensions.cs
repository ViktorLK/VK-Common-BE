using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.APIStandards.Behaviors;
using VK.Blocks.APIStandards.CorrelationId;
using VK.Blocks.APIStandards.Infrastructure;

namespace VK.Blocks.APIStandards.Extensions;

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

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }

    public static IServiceCollection AddCorrelationId(this IServiceCollection services, IConfiguration configuration) {

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
