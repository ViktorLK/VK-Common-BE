using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Guardrails.Injection.Internal;

/// <summary>
/// Injection Guard feature marker and registration hub.
/// </summary>
internal sealed partial class InjectionFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKInjectionOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKInjectionDetector, NoOpVKInjectionDetector>();
    }

    /// <summary>Add injection-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKInjectionOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
