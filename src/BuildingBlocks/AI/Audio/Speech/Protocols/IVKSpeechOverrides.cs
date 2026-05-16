namespace VK.Blocks.AI;

/// <summary>
/// Defines speech-specific parameters that can be overridden at the request level.
/// </summary>
public interface IVKSpeechOverrides :
    IVKAIProviderOverrides,
    IVKAIResilienceOverrides,
    IVKAIQuotaOverrides
{
    /// <summary>
    /// Gets the voice for speech generation.
    /// </summary>
    string? Voice { get; init; }

    /// <summary>
    /// Gets the desired audio format.
    /// </summary>
    string? AudioFormat { get; init; }

    /// <summary>
    /// Gets the speed of the generated speech (0.25 to 4.0).
    /// </summary>
    float? Speed { get; init; }

    /// <summary>
    /// Gets the pitch of the generated speech.
    /// </summary>
    float? Pitch { get; init; }

    /// <summary>
    /// Gets the speaking style.
    /// </summary>
    string? Style { get; init; }
}
