using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.VectorSearch.Rerank.Internal;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Vector Reranking feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKVectorSearchBlock), OptionsType = typeof(VKVectorRerankingOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class VectorRerankingFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKVectorRerankingOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKVectorReranker, NoOpReranker>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKVectorSearchPipelineStage, DefaultRerankStage>());
    }

    static partial void ValidateFeatureCustom(VKVectorRerankingOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
