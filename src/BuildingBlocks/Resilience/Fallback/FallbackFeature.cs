using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Resilience.Fallback.Internal;

namespace VK.Blocks.Resilience;

[VKFeature(typeof(VKResilienceBlock), OptionsType = typeof(VKFallbackOptions))]
internal sealed partial class FallbackFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKFallbackOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKFallbackHandler, LocalFallbackHandler>();
    }

    static partial void ValidateFeatureCustom(VKFallbackOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
