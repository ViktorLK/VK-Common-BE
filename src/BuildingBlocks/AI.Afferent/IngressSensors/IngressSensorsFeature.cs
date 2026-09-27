using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Afferent.IngressSensors.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// IngressSensors feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIAfferentBlock), OptionsType = typeof(VKIngressSensorsOptions))]
internal sealed partial class IngressSensorsFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKIngressSensorsOptions options)
    {
        _ = options;

        services.TryAddSingleton<IVKSystemEventDispatcher, DefaultSystemEventDispatcher>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, IngressSensorsPipelineStage>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKIngressSensorsOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
