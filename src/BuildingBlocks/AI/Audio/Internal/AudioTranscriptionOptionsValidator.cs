using System;
using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Audio.Internal;

/// <summary>
/// Validator for <see cref="VKAudioTranscriptionOptions"/>.
/// </summary>
internal sealed class AudioTranscriptionOptionsValidator : IValidateOptions<VKAudioTranscriptionOptions>
{
    public ValidateOptionsResult Validate(string? name, VKAudioTranscriptionOptions options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail("Audio transcription options cannot be null.");
        }

        if (options.Enabled)
        {
            if (string.IsNullOrWhiteSpace(options.ModelId))
            {
                return ValidateOptionsResult.Fail("ModelId is required when Audio Transcription is enabled.");
            }

            if (options.Timeout.HasValue && options.Timeout.Value <= TimeSpan.Zero)
            {
                return ValidateOptionsResult.Fail("Timeout must be greater than zero.");
            }
        }

        return ValidateOptionsResult.Success;
    }
}
