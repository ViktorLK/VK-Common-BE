using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Behaviors.Diagnostics.Internal;

/// <summary>
/// Source-generated logger messages for Psyche Pipeline execution.
/// </summary>
[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class BehaviorsDiagnostics
{
    [LoggerMessage(
        EventId = VKBehaviorsDiagnostics.ExecutionStartedEventId,
        Level = LogLevel.Information,
        Message = "Psyche pipeline execution started. SessionId: {SessionId}, TraceId: {TraceId}")]
    public static partial void ExecutionStarted(ILogger logger, string sessionId, string traceId);

    [LoggerMessage(
        EventId = VKBehaviorsDiagnostics.ExecutionCompletedEventId,
        Level = LogLevel.Information,
        Message = "Psyche pipeline execution completed successfully. TraceId: {TraceId}, Duration: {DurationMs}ms")]
    public static partial void ExecutionCompleted(ILogger logger, string traceId, double durationMs);

    [LoggerMessage(
        EventId = VKBehaviorsDiagnostics.ExecutionFailedEventId,
        Level = LogLevel.Error,
        Message = "Psyche pipeline execution failed. TraceId: {TraceId}, ErrorCode: {ErrorCode}, Message: {ErrorMessage}")]
    public static partial void ExecutionFailed(ILogger logger, string traceId, string errorCode, string errorMessage);

    [LoggerMessage(
        EventId = VKBehaviorsDiagnostics.PipelineStartedEventId,
        Level = LogLevel.Information,
        Message = "Psyche pipeline started. PersonaId: {PersonaId}, SessionId: {SessionId}, CorrelationId: {CorrelationId}")]
    public static partial void PipelineStarted(ILogger logger, VKPersonaId personaId, VKSessionId sessionId, string correlationId);

    [LoggerMessage(
        EventId = VKBehaviorsDiagnostics.PipelineCompletedEventId,
        Level = LogLevel.Information,
        Message = "Psyche pipeline completed successfully. CorrelationId: {CorrelationId}, Duration: {DurationMs}ms")]
    public static partial void PipelineCompleted(ILogger logger, string correlationId, double durationMs);

    [LoggerMessage(
        EventId = VKBehaviorsDiagnostics.PipelineFailedEventId,
        Level = LogLevel.Error,
        Message = "Psyche pipeline failed. CorrelationId: {CorrelationId}, ErrorCode: {ErrorCode}, Message: {ErrorMessage}")]
    public static partial void PipelineFailed(ILogger logger, string correlationId, string errorCode, string errorMessage);
}
