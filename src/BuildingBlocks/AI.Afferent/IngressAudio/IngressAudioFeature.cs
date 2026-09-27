using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Afferent.IngressAudio.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// IngressAudio feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIAfferentBlock), OptionsType = typeof(VKIngressAudioOptions))]
internal sealed partial class IngressAudioFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKIngressAudioOptions options)
    {
        _ = options;

        services.TryAddScoped<IVKIngressAudioService, DefaultIngressAudioService>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, IngressAudioPipelineStage>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKIngressAudioOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
