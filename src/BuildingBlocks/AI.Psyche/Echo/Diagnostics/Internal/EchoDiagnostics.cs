using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class EchoDiagnostics
{
    [LoggerMessage(
        EventId = VKEchoDiagnostics.EchoInitializedEventId,
        Level = LogLevel.Information,
        Message = "Echo short-term memory tracker initialized.")]
    public static partial void EchoInitialized(ILogger logger);

    [LoggerMessage(
        EventId = VKEchoDiagnostics.EchoRecordedEventId,
        Level = LogLevel.Debug,
        Message = "Recorded memory echo for session {SessionId}. Sender: {SenderRole}, Content length: {ContentLength}.")]
    public static partial void EchoRecorded(ILogger logger, VKSessionId sessionId, string senderRole, int contentLength);

    [LoggerMessage(
        EventId = VKEchoDiagnostics.EchoTrimmedEventId,
        Level = LogLevel.Information,
        Message = "Trimmed dialogue history for session {SessionId}. Original count: {OriginalCount}, Retained count: {RetainedCount}.")]
    public static partial void EchoTrimmed(ILogger logger, VKSessionId sessionId, int originalCount, int retainedCount);
}
