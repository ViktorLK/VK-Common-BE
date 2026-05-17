using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.AI.Audio.Internal;

/// <summary>
/// Audio pillar marker and registration hub.
/// </summary>
internal sealed partial class AudioFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKAudioOptions options)
    {
        _ = services;
        _ = options;
    }

    /// <summary>Add pillar-level validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKAudioOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
