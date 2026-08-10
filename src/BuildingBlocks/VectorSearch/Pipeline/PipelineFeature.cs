using VK.Blocks.VectorSearch.Pipeline.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Feature registration for the Search Pipeline.
/// </summary>
[VKFeature(typeof(VKVectorSearchBlock), OptionsType = typeof(VKPipelineOptions))]
internal sealed partial class PipelineFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKPipelineOptions options)
    {
        _ = options;
        services.TryAddScoped<IVKVectorSearchPipelineExecutor, DefaultVectorSearchPipelineExecutor>();
        services.TryAddScoped<IVKVectorSearchPipeline, DefaultVectorSearchPipeline>();
    }

    static partial void ValidateFeatureCustom(VKPipelineOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
