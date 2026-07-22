using VK.Blocks.AI.Audio.Speech.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Speech (TTS) feature marker and registration hub.
/// </summary>
[VKFeature(typeof(global::VK.Blocks.AI.AudioFeature), OptionsType = typeof(VKSpeechOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit, ArgsBaseType = typeof(IVKAIArgs))]
internal sealed partial class SpeechFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKSpeechOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKSpeechEngine, NoOpVKSpeechEngine>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKSpeechOptions options, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(options.Voice))
        {
            failures.Add("Voice must be specified for the Speech feature.");
        }
    }
}
