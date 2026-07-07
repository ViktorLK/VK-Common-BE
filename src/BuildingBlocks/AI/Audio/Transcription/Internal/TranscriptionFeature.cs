using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Audio.Transcription.Internal;

/// <summary>
/// Transcription (STT) feature marker and registration hub.
/// </summary>
internal sealed partial class TranscriptionFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKTranscriptionOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKTranscriptionEngine, NoOpVKTranscriptionEngine>();
    }

    /// <summary>Add STT-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateFeatureCustom(VKTranscriptionOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
