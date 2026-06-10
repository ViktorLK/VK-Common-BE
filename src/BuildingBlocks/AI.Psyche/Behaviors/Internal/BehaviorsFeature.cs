using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche.Behaviors.Pipeline.Internal;

namespace VK.Blocks.AI.Psyche.Behaviors.Internal;

/// <summary>
/// Psyche Pipeline feature marker and registration hub.
/// </summary>
internal sealed partial class BehaviorsFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKBehaviorsOptions options)
    {
        services.TryAddScoped<IVKPsychePipelineExecutor, DefaultPsychePipelineExecutor>();
        services.TryAddScoped<IVKPsychePipeline, DefaultPsychePipeline>();
    }

    // [SG Hook]
    static partial void ValidateCustom(VKBehaviorsOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
