using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.VectorSearch.Pipeline.Internal;

namespace VK.Blocks.VectorSearch.Pipeline.Internal;

/// <summary>
/// Feature registration for the Search Pipeline.
/// </summary>
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
