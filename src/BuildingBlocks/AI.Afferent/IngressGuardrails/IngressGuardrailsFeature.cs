using VK.Blocks.AI.Afferent.IngressGuardrails.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
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
        _ = services;
        _ = options;
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKIngressGuardrailsOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
