using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Psyche.Pipelines.Internal;

/// <summary>
/// Psyche Pipelines feature marker and registration hub.
/// </summary>
internal sealed partial class PipelinesFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKPipelinesOptions options)
    {
        services.TryAddScoped<IVKPsychePipelineExecutor, DefaultPsychePipelineExecutor>();
        services.TryAddScoped<IVKPsychePipeline, DefaultPsychePipeline>();
    }

    // [SG Hook]
    static partial void ValidateCustom(VKPipelinesOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
