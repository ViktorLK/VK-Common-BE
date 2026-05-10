using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Audio.Internal;

/// <summary>
/// Source-generated logger messages for the Audio features.
/// </summary>
internal static partial class AudioLog
{
    // --- Speech ---

    [LoggerMessage(
        EventId = 200,
        Level = LogLevel.Information,
        Message = "Audio speech engine initialized with model: {Model}")]
    public static partial void AudioSpeechEngineInitialized(ILogger logger, string? model);

    [LoggerMessage(
        EventId = 201,
        Level = LogLevel.Warning,
        Message = "Audio speech request failed: {Reason}")]
    public static partial void AudioSpeechRequestFailed(ILogger logger, string reason);

    // --- Transcription ---

    [LoggerMessage(
        EventId = 210,
        Level = LogLevel.Information,
        Message = "Audio transcription engine initialized with model: {Model}")]
    public static partial void AudioTranscriptionEngineInitialized(ILogger logger, string? model);

    [LoggerMessage(
        EventId = 211,
        Level = LogLevel.Warning,
        Message = "Audio transcription request failed: {Reason}")]
    public static partial void AudioTranscriptionRequestFailed(ILogger logger, string reason);
}
