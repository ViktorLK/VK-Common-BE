using VK.Blocks.AI.Efferent.EgressAudio.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;

/// <summary>
/// EgressAudio feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIEfferentBlock), OptionsType = typeof(VKEgressAudioOptions))]
internal sealed partial class EgressAudioFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKEgressAudioOptions options)
    {
        if (!options.Enabled)
            return;

        services.TryAddScoped<IVKEgressAudioService, DefaultEgressAudioService>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, EgressAudioPipelineStage>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKEgressAudioOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
