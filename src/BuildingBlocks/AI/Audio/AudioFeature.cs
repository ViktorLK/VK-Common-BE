using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Audio pillar marker and registration hub.
/// </summary>
[VKFeature(typeof(global::VK.Blocks.AI.VKAIBlock), OptionsType = typeof(VKAudioOptions))]
internal sealed partial class AudioFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKAudioOptions options)
    {
        _ = services;
        _ = options;
    }

    /// <summary>Add pillar-level validation logic here</summary>
    // [SG Hook]
    static partial void ValidateFeatureCustom(VKAudioOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
