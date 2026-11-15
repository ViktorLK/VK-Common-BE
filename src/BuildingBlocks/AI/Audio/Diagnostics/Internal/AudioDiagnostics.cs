using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Audio.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIBlock>]
internal static partial class AudioDiagnostics
{
    [LoggerMessage(
        EventId = VKAudioDiagnosticsConstants.AudioInitializedEventId,
        Level = LogLevel.Debug,
        Message = "Audio feature initialized.")]
    public static partial void AudioInitialized(ILogger logger);

    [LoggerMessage(
        EventId = VKAudioDiagnosticsConstants.AudioSpeechEngineInitializedEventId,
        Level = LogLevel.Information,
        Message = "Audio speech engine initialized with model: {Model}")]
    public static partial void AudioSpeechEngineInitialized(ILogger logger, string? model);

    [LoggerMessage(
        EventId = VKAudioDiagnosticsConstants.AudioSpeechRequestFailedEventId,
        Level = LogLevel.Warning,
        Message = "Audio speech request failed: {Reason}")]
    public static partial void AudioSpeechRequestFailed(ILogger logger, string reason);

    [LoggerMessage(
        EventId = VKAudioDiagnosticsConstants.AudioTranscriptionEngineInitializedEventId,
        Level = LogLevel.Information,
        Message = "Audio transcription engine initialized with model: {Model}")]
    public static partial void AudioTranscriptionEngineInitialized(ILogger logger, string? model);

    [LoggerMessage(
        EventId = VKAudioDiagnosticsConstants.AudioTranscriptionRequestFailedEventId,
        Level = LogLevel.Warning,
        Message = "Audio transcription request failed: {Reason}")]
    public static partial void AudioTranscriptionRequestFailed(ILogger logger, string reason);
}
