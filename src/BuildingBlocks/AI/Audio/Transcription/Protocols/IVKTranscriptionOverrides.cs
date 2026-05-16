namespace VK.Blocks.AI;

/// <summary>
/// Defines transcription-specific parameters that can be overridden at the request level.
/// </summary>
public interface IVKTranscriptionOverrides :
    IVKAIProviderOverrides,
    IVKAIResilienceOverrides,
    IVKAIQuotaOverrides
{
    /// <summary>
    /// Gets the language for transcription.
    /// </summary>
    string? Language { get; init; }

    /// <summary>
    /// Gets a value indicating whether to translate the audio into English.
    /// </summary>
    bool? Translate { get; init; }

    /// <summary>
    /// Gets a value indicating whether to enable speaker diarization.
    /// </summary>
    bool? EnableDiarization { get; init; }

    /// <summary>
    /// Gets a value indicating whether to include timestamps.
    /// </summary>
    bool? EnableTimestamps { get; init; }

    /// <summary>
    /// Gets the sampling temperature.
    /// </summary>
    float? Temperature { get; init; }

    /// <summary>
    /// Gets the response format.
    /// </summary>
    string? ResponseFormat { get; init; }
}
