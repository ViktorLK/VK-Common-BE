using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Afferent.Environment.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Environment feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIAfferentBlock), OptionsType = typeof(VKEnvironmentOptions))]
internal sealed partial class EnvironmentFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKEnvironmentOptions options)
    {
        _ = options;

        services.TryAddScoped<IVKEnvironmentPerceptionProvider, DefaultEnvironmentPerceptionProvider>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, EnvironmentPipelineStage>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKEnvironmentOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
