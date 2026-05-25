using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Audio.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIBlock>]
internal static partial class AudioDiagnostics
{
    [LoggerMessage(
        EventId = VKAudioDiagnosticTokens.AudioInitializedEventId,
        Level = LogLevel.Debug,
        Message = "Audio feature initialized.")]
    public static partial void AudioInitialized(ILogger logger);
}
