using VK.Blocks.AI.Efferent.EgressGuardrails.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;

/// <summary>
/// EgressGuardrails feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIEfferentBlock), OptionsType = typeof(VKEgressGuardrailsOptions))]
internal sealed partial class EgressGuardrailsFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKEgressGuardrailsOptions options)
    {
        if (!options.Enabled)
            return;

        services.TryAddScoped<IVKEgressGuardrail, DefaultEgressGuardrail>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, EgressGuardrailsPipelineStage>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKEgressGuardrailsOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
