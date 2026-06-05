using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pipeline.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class PipelineDiagnostics
{
    [LoggerMessage(
        EventId = VKPipelineDiagnostics.PipelineStartedEventId,
        Level = LogLevel.Information,
        Message = "Psyche pipeline started. PersonaId: {PersonaId}, SessionId: {SessionId}, CorrelationId: {CorrelationId}")]
    public static partial void PipelineStarted(ILogger logger, VKPersonaId personaId, VKSessionId sessionId, string correlationId);

    [LoggerMessage(
        EventId = VKPipelineDiagnostics.PipelineCompletedEventId,
        Level = LogLevel.Information,
        Message = "Psyche pipeline completed successfully. CorrelationId: {CorrelationId}, Duration: {DurationMs}ms")]
    public static partial void PipelineCompleted(ILogger logger, string correlationId, double durationMs);

    [LoggerMessage(
        EventId = VKPipelineDiagnostics.PipelineFailedEventId,
        Level = LogLevel.Error,
        Message = "Psyche pipeline failed. CorrelationId: {CorrelationId}, ErrorCode: {ErrorCode}, Message: {ErrorMessage}")]
    public static partial void PipelineFailed(ILogger logger, string correlationId, string errorCode, string errorMessage);
}
