using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.VectorSearch.SemanticCache.Internal;

/// <summary>
/// Semantic Cache feature marker and registration hub.
/// </summary>
internal sealed partial class SemanticCacheFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKSemanticCacheOptions options)
    {
        _ = options;
        services.TryAddScoped<IVKSemanticCacheService, DefaultSemanticCacheService>();
        services.TryAddScoped<IVKVectorSearchBeforePipelineStage, DefaultSemanticCacheStage>();
        services.TryAddScoped<IVKVectorSearchAfterPipelineStage, SemanticCacheWriteStage>();
    }

    static partial void ValidateCustom(VKSemanticCacheOptions options, List<string> failures)
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
