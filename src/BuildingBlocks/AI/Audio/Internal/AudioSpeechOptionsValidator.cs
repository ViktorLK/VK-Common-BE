using System;
using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Audio.Internal;

/// <summary>
/// Validator for <see cref="VKAudioSpeechOptions"/>.
/// </summary>
internal sealed class AudioSpeechOptionsValidator : IValidateOptions<VKAudioSpeechOptions>
{
    public ValidateOptionsResult Validate(string? name, VKAudioSpeechOptions options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail("Audio speech options cannot be null.");
        }

        if (options.Enabled)
        {
            if (string.IsNullOrWhiteSpace(options.ModelId))
            {
                return ValidateOptionsResult.Fail("ModelId is required when Audio Speech is enabled.");
            }

            if (options.Timeout.HasValue && options.Timeout.Value <= TimeSpan.Zero)
            {
                return ValidateOptionsResult.Fail("Timeout must be greater than zero.");
            }
        }

        return ValidateOptionsResult.Success;
    }
}
