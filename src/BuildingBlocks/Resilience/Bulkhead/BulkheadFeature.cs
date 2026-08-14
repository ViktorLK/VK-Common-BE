using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Resilience.Bulkhead.Internal;

namespace VK.Blocks.Resilience;

[VKFeature(typeof(VKResilienceBlock), OptionsType = typeof(VKBulkheadOptions))]
internal sealed partial class BulkheadFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKBulkheadOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKBulkhead, LocalBulkhead>();
    }

    static partial void ValidateFeatureCustom(VKBulkheadOptions options, System.Collections.Generic.List<string> failures)
    {
        if (options.MaxParallelization <= 0)
        {
            failures.Add("MaxParallelization must be greater than zero.");
        }

        if (options.MaxQueuedItems < 0)
        {
            failures.Add("MaxQueuedItems must be non-negative.");
        }
    }
}
