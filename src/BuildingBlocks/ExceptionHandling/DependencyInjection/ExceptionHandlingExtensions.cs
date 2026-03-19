using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.ExceptionHandling.Abstractions;
using VK.Blocks.ExceptionHandling.Factories;
using VK.Blocks.ExceptionHandling.Handlers;
using VK.Blocks.ExceptionHandling.Options;
using VK.Blocks.ExceptionHandling.Pipeline;

namespace VK.Blocks.ExceptionHandling.DependencyInjection;

/// <summary>
/// Extension methods for registering exception handling services.
/// </summary>
public static class ExceptionHandlingExtensions
{
    /// <summary>
    /// Adds exception handling services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    public static IExceptionHandlingBuilder AddExceptionHandling(
        this IServiceCollection services,
        Action<ExceptionHandlingOptions>? configure = null)
    {
        if (configure != null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton<IProblemDetailsFactory, ProblemDetailsFactory>();
        services.TryAddScoped<IExceptionHandlerPipeline, ExceptionHandlerPipeline>();

        var builder = new ExceptionHandlingBuilder(services);

        // Register default handlers in order of execution
        builder.AddHandler<ValidationExceptionHandler>();
        builder.AddHandler<UnauthorizedExceptionHandler>();
        builder.AddHandler<NotFoundExceptionHandler>();
        builder.AddHandler<BaseExceptionHandler>();
        builder.AddHandler<DefaultExceptionHandler>();

        return builder;
    }

    /// <summary>
    /// Uses the exception handling middleware in the specified <see cref="IApplicationBuilder"/>.
    /// </summary>
    public static IApplicationBuilder UseVKExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
