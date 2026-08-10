using VK.Blocks.VectorSearch.SemanticCache.Internal;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Semantic Cache feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKVectorSearchBlock), OptionsType = typeof(VKSemanticCacheOptions))]
internal sealed partial class SemanticCacheFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKSemanticCacheOptions options)
    {
        _ = options;
        services.TryAddScoped<IVKSemanticCacheService, DefaultSemanticCacheService>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKVectorSearchPipelineStage, DefaultSemanticCacheStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKVectorSearchPipelineStage, SemanticCacheWriteStage>());
    }

    static partial void ValidateFeatureCustom(VKSemanticCacheOptions options, List<string> failures)
    {
        if (options.ScoreThreshold is < 0.0 or > 1.0)
        {
            failures.Add("ScoreThreshold must be between 0.0 and 1.0.");
        }
        if (options.Ttl <= TimeSpan.Zero)
        {
            failures.Add("Ttl must be greater than zero.");
        }
        if (string.IsNullOrWhiteSpace(options.CollectionName))
        {
            failures.Add("CollectionName cannot be empty.");
        }
    }
}
