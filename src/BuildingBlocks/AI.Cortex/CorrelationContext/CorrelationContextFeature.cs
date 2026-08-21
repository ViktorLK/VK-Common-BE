using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Cortex.CorrelationContext.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Feature marker and DI registration for Correlation Context slice.
/// </summary>
[VKFeature(typeof(VKAICortexBlock), OptionsType = typeof(VKCorrelationContextOptions))]
internal sealed partial class CorrelationContextFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKCorrelationContextOptions options)
    {
        services.TryAddSingleton<IVKCortexCorrelationAccessor, AsyncLocalCorrelationAccessor>();
    }
}
