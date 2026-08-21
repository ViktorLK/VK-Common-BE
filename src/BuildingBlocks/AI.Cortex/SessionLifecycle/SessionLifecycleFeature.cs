using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Cortex.SessionLifecycle.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Feature marker and DI registration for Session Lifecycle slice.
/// </summary>
[VKFeature(typeof(VKAICortexBlock), OptionsType = typeof(VKSessionLifecycleOptions))]
internal sealed partial class SessionLifecycleFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKSessionLifecycleOptions options)
    {
        services.TryAddScoped<IVKSessionLifecycleCoordinator, DefaultSessionLifecycleCoordinator>();
    }
}
