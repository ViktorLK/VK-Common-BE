using VK.Blocks.VectorSearch.Retrieval.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Feature registration for the Retrieval (Search) capability.
/// </summary>
[VKFeature(typeof(VKVectorSearchBlock), OptionsType = typeof(VKRetrievalOptions))]
internal sealed partial class RetrievalFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKRetrievalOptions options)
    {
        _ = options;

        services.TryAddScoped<DefaultVectorSearchStrategy>();
        services.TryAddScoped<DefaultKeywordSearchStrategy>();
        services.TryAddScoped<DefaultHybridSearchStrategy>();

        services.TryAddScoped<IVKSearchStrategy, DefaultHybridSearchStrategy>();
        services.TryAddScoped<IVKRetrievalStore, DefaultRetrievalStore>();
    }

    static partial void ValidateFeatureCustom(VKRetrievalOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
