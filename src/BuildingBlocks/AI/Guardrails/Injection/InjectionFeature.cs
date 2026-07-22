using VK.Blocks.AI.Guardrails.Injection.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Injection Guard feature marker and registration hub.
/// </summary>
[VKFeature(typeof(global::VK.Blocks.AI.GuardrailsFeature), OptionsType = typeof(VKInjectionOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class InjectionFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKInjectionOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKInjectionDetector, NoOpVKInjectionDetector>();
    }

    /// <summary>Add injection-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateFeatureCustom(VKInjectionOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
