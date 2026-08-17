using VK.Blocks.ExceptionHandling.Resolution;
using VK.Blocks.ExceptionHandling.Resolution.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.ExceptionHandling;

/// <summary>
/// Feature marker for the Resolution feature of the ExceptionHandling building block.
/// </summary>
[VKFeature(typeof(VKExceptionHandlingBlock), OptionsType = typeof(VKResolutionOptions))]
internal sealed partial class ResolutionFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKResolutionOptions options)
    {
        // Core Services
        services.TryAddScoped<IVKExceptionHandlerPipeline, ExceptionHandlerPipeline>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKExceptionHandler, DefaultExceptionHandler>());
    }
}
