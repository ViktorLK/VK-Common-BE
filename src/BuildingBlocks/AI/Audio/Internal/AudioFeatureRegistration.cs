using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Audio.Internal;

/// <summary>
/// Handles the registration of the Audio features (Speech and Transcription).
/// </summary>
internal static class AudioFeatureRegistration
{
    public static IVKAIBuilder RegisterSpeech(
        IVKAIBuilder builder,
        Func<VKAudioSpeechOptions, VKAudioSpeechOptions>? transform = null)
    {
        IServiceCollection services = builder.Services;

        if (services.IsVKBlockRegistered<AudioSpeechFeature>())
        {
            return builder;
        }

        VKAudioSpeechOptions options = services.AddVKBlockOptions<VKAudioSpeechOptions>(builder.Configuration!, transform);
        services.AddVKBlockMarker<AudioSpeechFeature>();
        services.TryAddEnumerableSingleton<IValidateOptions<VKAudioSpeechOptions>, AudioSpeechOptionsValidator>();

        if (!options.Enabled)
        {
            return builder;
        }

        // Feature services registration
        services.AddScoped<IVKAudioSpeechOptionsProvider, VKAudioSpeechDefaultOptionsProvider>();

        return builder;
    }

    public static IVKAIBuilder RegisterTranscription(
        IVKAIBuilder builder,
        Func<VKAudioTranscriptionOptions, VKAudioTranscriptionOptions>? transform = null)
    {
        IServiceCollection services = builder.Services;

        if (services.IsVKBlockRegistered<AudioTranscriptionFeature>())
        {
            return builder;
        }

        VKAudioTranscriptionOptions options = services.AddVKBlockOptions<VKAudioTranscriptionOptions>(builder.Configuration!, transform);
        services.AddVKBlockMarker<AudioTranscriptionFeature>();
        services.TryAddEnumerableSingleton<IValidateOptions<VKAudioTranscriptionOptions>, AudioTranscriptionOptionsValidator>();

        if (!options.Enabled)
        {
            return builder;
        }

        // Feature services registration
        services.AddScoped<IVKAudioTranscriptionOptionsProvider, VKAudioTranscriptionDefaultOptionsProvider>();

        return builder;
    }
}
