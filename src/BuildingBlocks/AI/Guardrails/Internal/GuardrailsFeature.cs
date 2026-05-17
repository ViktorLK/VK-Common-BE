using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.AI.Guardrails.Internal;

/// <summary>
/// Guardrails feature marker and registration hub.
/// </summary>
internal sealed partial class GuardrailsFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKGuardrailsOptions options)
    {
        _ = services;
        _ = options;
    }

    /// <summary>Add guardrail-level validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKGuardrailsOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
