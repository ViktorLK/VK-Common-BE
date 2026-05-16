using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Audio.Speech.Internal;

/// <summary>
/// Source-generated logger messages for the Audio Speech feature.
/// </summary>
// [SG Logger] - This class is automatically implemented by the Source Generator for high-performance logging.
internal static partial class AudioSpeechLog
{
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
}
