using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Resilience.Retry.Internal;

namespace VK.Blocks.Resilience;

[VKFeature(typeof(VKResilienceBlock), OptionsType = typeof(VKRetryOptions))]
internal sealed partial class RetryFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKRetryOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKRetryExecutor, LocalRetryExecutor>();
    }

    static partial void ValidateFeatureCustom(VKRetryOptions options, System.Collections.Generic.List<string> failures)
    {
        if (options.MaxRetries < 0)
        {
            failures.Add("MaxRetries must be non-negative.");
        }

        if (options.InitialDelay <= TimeSpan.Zero)
        {
            failures.Add("InitialDelay must be greater than zero.");
        }

        if (options.MaxDelay < options.InitialDelay)
        {
            failures.Add("MaxDelay must be greater than or equal to InitialDelay.");
        }

        if (options.BackoffMultiplier <= 0)
        {
            failures.Add("BackoffMultiplier must be greater than zero.");
        }
    }
}
