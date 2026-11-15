using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Audio.Transcription.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Transcription (STT) feature marker and registration hub.
/// </summary>
[VKFeature(typeof(AudioFeature), OptionsType = typeof(VKTranscriptionOptions))]
internal sealed partial class TranscriptionFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKTranscriptionOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKTranscriptionEngine, NoOpTranscriptionEngine>();
    }

    /// <summary>Add STT-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateFeatureCustom(VKTranscriptionOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
