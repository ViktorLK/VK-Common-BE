using VK.Blocks.AI.Guardrails.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Guardrails feature marker and registration hub.
/// </summary>
[VKFeature(typeof(global::VK.Blocks.AI.VKAIBlock), OptionsType = typeof(VKGuardrailsOptions))]
internal sealed partial class GuardrailsFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKGuardrailsOptions options)
    {
        _ = services;
        _ = options;
    }

    /// <summary>Add guardrail-level validation logic here</summary>
    // [SG Hook]
    static partial void ValidateFeatureCustom(VKGuardrailsOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
