using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.VectorSearch.Retrieval.Internal;

namespace VK.Blocks.VectorSearch.Retrieval.Internal;

/// <summary>
/// Feature registration for the Retrieval (Search) capability.
/// </summary>
internal sealed partial class RetrievalFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKRetrievalOptions options)
    {
        _ = options;

        services.TryAddScoped<DefaultVectorSearchStrategy>();
        services.TryAddScoped<DefaultKeywordSearchStrategy>();
        services.TryAddScoped<DefaultHybridSearchStrategy>();

        services.TryAddScoped<IVKSearchStrategy, DefaultHybridSearchStrategy>();
        services.TryAddScoped<IVKRetrievalStore, DefaultRetrievalStore>();
    }

    static partial void ValidateCustom(VKRetrievalOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
