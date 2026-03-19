using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Validation.Abstractions;
using VK.Blocks.Validation.Options;
using VK.Blocks.Validation.Pipeline;
using VK.Blocks.Validation.Validators;

namespace VK.Blocks.Validation.DependencyInjection;

/// <summary>
/// Extension methods for setting up validation services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Adds validation services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    public static IServiceCollection AddVKValidation(
        this IServiceCollection services,
        Action<ValidationOptions>? configure = null)
    {
        var options = new ValidationOptions();
        configure?.Invoke(options);
        
        services.Configure(configure ?? (_ => { }));

        if (options.EnableDataAnnotations)
        {
            services.AddSingleton<IValidator, DataAnnotationsValidator>();
        }

        if (options.EnableFluentValidation)
        {
            services.AddSingleton<IValidator, FluentValidationValidator>();
        }

        services.TryAddScoped<IValidationPipeline, ValidationPipeline>();

        return services;
    }

    /// <summary>
    /// Adds the validation middleware to the specified <see cref="IApplicationBuilder" />.
    /// </summary>
    public static IApplicationBuilder UseVKValidation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ValidationMiddleware>();
    }
}
