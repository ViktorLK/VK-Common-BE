using VK.Blocks.AI.Efferent.EgressText.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;

/// <summary>
/// EgressText feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIEfferentBlock), OptionsType = typeof(VKEgressTextOptions))]
internal sealed partial class EgressTextFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKEgressTextOptions options)
    {
        if (!options.Enabled)
            return;

        services.TryAddScoped<IVKEgressTextFormatter, DefaultEgressTextFormatter>();
        services.TryAddScoped<IVKEgressPacer, DefaultEgressPacer>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, EgressTextPipelineStage>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKEgressTextOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
