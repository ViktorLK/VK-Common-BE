using VK.Blocks.AI.Guardrails.Privacy.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Privacy Guard feature marker and registration hub.
/// </summary>
[VKFeature(typeof(global::VK.Blocks.AI.GuardrailsFeature), OptionsType = typeof(VKPrivacyOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class PrivacyFeature
{
    /// <summary>Add privacy services here</summary>
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKPrivacyOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKPrivacyFilter, NoOpVKPrivacyFilter>();
    }
}
