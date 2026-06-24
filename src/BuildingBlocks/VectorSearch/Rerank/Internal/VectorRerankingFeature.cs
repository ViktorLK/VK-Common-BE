using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.VectorSearch;
using VK.Blocks.VectorSearch.Rerank.Internal;

namespace VK.Blocks.VectorSearch.VectorReranking.Internal;

/// <summary>
/// Vector Reranking feature marker and registration hub.
/// </summary>
internal sealed partial class VectorRerankingFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKVectorRerankingOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKVectorReranker, NoOpReranker>();
        services.TryAddScoped<IVKVectorSearchAfterPipelineStage, DefaultRerankStage>();
    }

    static partial void ValidateCustom(VKVectorRerankingOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
