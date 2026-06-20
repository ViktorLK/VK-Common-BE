using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class EchoDiagnostics
{
    [LoggerMessage(
        EventId = VKEchoDiagnosticsConstants.Logs.EchoInitialized,
        Level = LogLevel.Information,
        Message = "Echo short-term memory tracker initialized.")]
    public static partial void EchoInitialized(this ILogger logger);

    [LoggerMessage(
        EventId = VKEchoDiagnosticsConstants.Logs.EchoRecorded,
        Level = LogLevel.Debug,
        Message = "Recorded memory echo for session {SessionId}. Sender: {SenderRole}, Content length: {ContentLength}.")]
    public static partial void EchoRecorded(this ILogger logger, VKSessionId sessionId, string senderRole, int contentLength);

    [LoggerMessage(
        EventId = VKEchoDiagnosticsConstants.Logs.EchoTrimmed,
        Level = LogLevel.Information,
        Message = "Trimmed dialogue history for session {SessionId}. Original count: {OriginalCount}, Retained count: {RetainedCount}.")]
    public static partial void EchoTrimmed(this ILogger logger, VKSessionId sessionId, int originalCount, int retainedCount);
}
