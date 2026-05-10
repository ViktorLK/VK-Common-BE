using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Audio.Internal;

/// <summary>
/// Handles the registration of the Audio features (Speech and Transcription).
/// </summary>
internal static class AudioFeatureRegistration
{
    public static IVKAIBuilder Register(IVKAIBuilder builder)
    {
        IServiceCollection services = builder.Services;

        // 1. Idempotency Check
        if (services.IsVKBlockRegistered<AudioFeature>())
        {
            return builder;
        }

        // 2. Options Registration
        VKAudioSpeechOptions speechOptions = services.AddVKBlockOptions<VKAudioSpeechOptions>(builder.Configuration!);
        VKAudioTranscriptionOptions transcriptionOptions = services.AddVKBlockOptions<VKAudioTranscriptionOptions>(builder.Configuration!);

        // 3. Mark-Self
        services.AddVKBlockMarker<AudioFeature>();

        // 4. Options Validation
        services.TryAddEnumerableSingleton<IValidateOptions<VKAudioSpeechOptions>, AudioSpeechOptionsValidator>();
        services.TryAddEnumerableSingleton<IValidateOptions<VKAudioTranscriptionOptions>, AudioTranscriptionOptionsValidator>();

        // 5. Feature Toggles
        if (!speechOptions.Enabled && !transcriptionOptions.Enabled)
        {
            return builder;
        }

        // 6. Core Services
        // Implementations would go here.

        return builder;
    }
}
