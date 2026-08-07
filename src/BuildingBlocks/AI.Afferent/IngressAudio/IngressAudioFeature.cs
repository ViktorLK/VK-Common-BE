using VK.Blocks.AI.Afferent.IngressAudio.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
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
        _ = services;
        _ = options;
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKIngressAudioOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
