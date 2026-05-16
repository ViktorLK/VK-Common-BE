using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Vectorics.SemanticCache.Internal;

/// <summary>
/// Semantic Cache feature marker and registration hub.
/// </summary>
internal sealed partial class SemanticCacheFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKSemanticCacheOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKSemanticCache, NoOpVKSemanticCache>();
    }

    // [SG Hook]
    static partial void ValidateCustom(VKSemanticCacheOptions options, System.Collections.Generic.List<string> failures)
    {
        if (options.MinSimilarity is < 0 or > 1)
        {
            failures.Add("MinSimilarity must be between 0 and 1.");
        }

        if (options.Ttl.TotalSeconds < 0)
        {
            failures.Add("Ttl cannot be negative.");
        }
    }
}
