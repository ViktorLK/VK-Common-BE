using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Audio.Transcription.Internal;

/// <summary>
/// Source-generated logger messages for the Audio Transcription feature.
/// </summary>
// [SG Logger] - This class is automatically implemented by the Source Generator for high-performance logging.
internal static partial class AudioTranscriptionLog
{
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
