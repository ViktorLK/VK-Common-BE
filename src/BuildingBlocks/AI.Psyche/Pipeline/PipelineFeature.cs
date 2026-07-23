using VK.Blocks.AI.Psyche.Pipeline.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Psyche Pipeline feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIPsycheBlock), OptionsType = typeof(VKPipelineOptions))]
internal sealed partial class PipelineFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKPipelineOptions options)
    {
        services.TryAddScoped<IVKPsychePipelineExecutor, DefaultPsychePipelineExecutor>();
        services.TryAddScoped<IVKPsychePipeline, DefaultPsychePipeline>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKPipelineOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
