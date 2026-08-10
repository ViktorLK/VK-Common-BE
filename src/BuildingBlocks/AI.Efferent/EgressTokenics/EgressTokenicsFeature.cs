using VK.Blocks.AI.Efferent.EgressTokenics.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;

/// <summary>
/// EgressTokenics feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIEfferentBlock), OptionsType = typeof(VKEgressTokenicsOptions))]
internal sealed partial class EgressTokenicsFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKEgressTokenicsOptions options)
    {
        if (!options.Enabled)
            return;

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, EgressTokenicsPipelineStage>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKEgressTokenicsOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
