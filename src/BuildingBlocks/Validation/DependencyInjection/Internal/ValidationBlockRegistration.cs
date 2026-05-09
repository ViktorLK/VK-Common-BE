using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.Validation.Pipelines.Internal;
using VK.Blocks.Validation.Validators.Internal;

namespace VK.Blocks.Validation.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the Validation building block.
/// </summary>
internal static class ValidationBlockRegistration
{
    public static IVKValidationBuilder Register(IServiceCollection services, IConfiguration configuration, Func<VKValidationOptions, VKValidationOptions>? transform = null)
    {
        var builder = new ValidationBuilder(services, configuration);

        // 1. Check-Self & Prerequisite
        if (services.IsVKBlockRegistered<VKValidationBlock>())
        {
            return builder;
        }

        // 2. Options Registration
        var options = services.AddVKBlockOptions<VKValidationOptions>(configuration, transform);

        // 3. Mark-Self
        services.AddVKBlockMarker<VKValidationBlock>();

        // 4. Options Validation
        services.TryAddEnumerableSingleton<IValidateOptions<VKValidationOptions>, ValidationOptionsValidator>();

        // 5. Diagnostics/Static Metadata
        // BB.04: Use static partial class [VKBlockDiagnostics] for telemetry.
        // Static diagnostics do not require DI registration.

        // 6. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 7. Core Services
        if (options.EnableDataAnnotations)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IVKValidator, DataAnnotationsValidator>());
        }

        services.TryAddScoped<IVKValidationPipeline, ValidationPipeline>();

        return builder;
    }
}

