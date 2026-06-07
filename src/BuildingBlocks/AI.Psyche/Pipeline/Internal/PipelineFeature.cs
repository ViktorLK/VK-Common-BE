using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Psyche.Pipeline.Internal;

/// <summary>
/// Pipeline feature marker and registration hub.
/// </summary>
internal sealed partial class PipelineFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKPipelineOptions options)
    {
        _ = options;
        services.TryAddScoped<IVKPsychePipeline, DefaultPsychePipeline>();
    }

    // [SG Hook]
    static partial void ValidateCustom(VKPipelineOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
