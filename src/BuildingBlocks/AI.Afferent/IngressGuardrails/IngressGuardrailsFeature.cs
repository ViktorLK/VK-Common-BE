using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Afferent.IngressGuardrails.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// IngressGuardrails feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIAfferentBlock), OptionsType = typeof(VKIngressGuardrailsOptions))]
internal sealed partial class IngressGuardrailsFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKIngressGuardrailsOptions options)
    {
        _ = options;

        services.TryAddScoped<IVKIngressGuardrail, DefaultIngressGuardrail>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, IngressGuardrailsPipelineStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, IngressVisionPipelineStage>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKIngressGuardrailsOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
