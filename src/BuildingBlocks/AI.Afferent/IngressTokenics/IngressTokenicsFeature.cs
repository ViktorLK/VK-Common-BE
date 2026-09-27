using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Afferent.IngressTokenics.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// IngressTokenics feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIAfferentBlock), OptionsType = typeof(VKIngressTokenicsOptions))]
internal sealed partial class IngressTokenicsFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKIngressTokenicsOptions options)
    {
        _ = options;

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, IngressTokenicsPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, IngressRateLimitPipelineStage>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKIngressTokenicsOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
