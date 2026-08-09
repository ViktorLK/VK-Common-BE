using VK.Blocks.AI.Efferent.EgressActuators.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;

/// <summary>
/// EgressActuators feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIEfferentBlock), OptionsType = typeof(VKEgressActuatorsOptions))]
internal sealed partial class EgressActuatorsFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKEgressActuatorsOptions options)
    {
        if (!options.Enabled)
            return;

        services.TryAddScoped<IVKEgressActuators, DefaultEgressActuators>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, EgressActuatorsPipelineStage>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKEgressActuatorsOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
