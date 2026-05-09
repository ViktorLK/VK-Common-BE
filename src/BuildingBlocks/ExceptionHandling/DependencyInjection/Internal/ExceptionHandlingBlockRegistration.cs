using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.ExceptionHandling.Pipelines.Internal;

namespace VK.Blocks.ExceptionHandling.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the ExceptionHandling block.
/// </summary>
internal static class ExceptionHandlingBlockRegistration
{
    public static IVKExceptionHandlingBuilder Register(IServiceCollection services, IConfiguration configuration)
    {
        var builder = new ExceptionHandlingBuilder(services);

        // 1. Check-Self & Prerequisite
        if (services.IsVKBlockRegistered<VKExceptionHandlingBlock>())
        {
            return builder;
        }

        // 2. Options Registration
        var options = services.AddVKBlockOptions<VKExceptionHandlingOptions>(configuration);

        // 3. Mark-Self
        services.AddVKBlockMarker<VKExceptionHandlingBlock>();

        // 4. Options Validation
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<VKExceptionHandlingOptions>, ExceptionHandlingOptionsValidator>());

        // 5. Diagnostics
        // (Handled by [VKBlockDiagnostics] static members)

        // 6. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 7. Core Services
        services.TryAddScoped<IVKExceptionHandlerPipeline, ExceptionHandlerPipeline>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKExceptionHandler, DefaultExceptionHandler>());

        return builder;
    }
}
