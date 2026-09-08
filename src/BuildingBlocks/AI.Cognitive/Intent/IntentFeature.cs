using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Cognitive.Intent.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Feature marker and registration hook for the Intent triage and classification feature.
/// Following BB.02 and BB.06.
/// </summary>
[VKFeature(typeof(VKAICognitiveBlock), OptionsType = typeof(VKIntentOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class IntentFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKIntentOptions options) // [SG Hook]
    {
        services.TryAddScoped<IVKIntentRouter, IndustrialIntentRouter>();
        services.TryAddSingleton<IVKIntentArbiter, DefaultIntentArbiter>();
        services.TryAddSingleton<DefaultIntentOrchestrator>();

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKOrchestrationPipelineStage, DefaultIntentPipelineStage>());
    }

    static partial void ValidateFeatureCustom(VKIntentOptions options, List<string> failures) // [SG Hook]
    {
        if (options.Enabled && options.ConfidenceThreshold < 0)
        {
            failures.Add("Intent.ConfidenceThreshold must be non-negative.");
        }
    }
}
