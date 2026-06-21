using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Ingest.VecSink.Internal;

/// <summary>
/// Service registration and options validation for VecSink feature.
/// </summary>
internal sealed partial class VecSinkFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKVecSinkOptions options)
    {
        services.TryAddScoped<IVKVecIndexingSink, AIVectorStoreIndexingSink>();
    }

    // [SG Hook]
    static partial void ValidateCustom(VKVecSinkOptions options, System.Collections.Generic.List<string> failures)
    {
        if (options.BatchSize <= 0)
        {
            failures.Add("BatchSize must be greater than 0.");
        }

        if (options.MaxConcurrency <= 0)
        {
            failures.Add("MaxConcurrency must be greater than 0.");
        }
    }
}
