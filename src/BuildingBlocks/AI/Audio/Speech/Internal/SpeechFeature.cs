using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Audio.Speech.Internal;

/// <summary>
/// Speech (TTS) feature marker and registration hub.
/// </summary>
internal sealed partial class SpeechFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKSpeechOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKSpeechEngine, NoOpVKSpeechEngine>();
    }

    // [SG Hook]
    static partial void ValidateCustom(VKSpeechOptions options, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(options.Voice))
        {
            failures.Add("Voice must be specified for the Speech feature.");
        }
    }
}
