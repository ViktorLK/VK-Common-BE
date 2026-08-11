using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche.Pipeline.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Psyche Pipeline feature marker and registration hub.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Feature marker and DI registration hub containing no business logic.")]
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
