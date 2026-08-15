using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Resilience.Timeout.Internal;

namespace VK.Blocks.Resilience;

[VKFeature(typeof(VKResilienceBlock), OptionsType = typeof(VKTimeoutOptions))]
internal sealed partial class TimeoutFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKTimeoutOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKTimeoutExecutor, LocalTimeoutExecutor>();
    }

    static partial void ValidateFeatureCustom(VKTimeoutOptions options, System.Collections.Generic.List<string> failures)
    {
        if (options.Duration <= TimeSpan.Zero)
        {
            failures.Add("Duration must be greater than zero.");
        }
    }
}
