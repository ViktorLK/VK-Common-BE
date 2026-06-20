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
        EventId = VKBehaviorsDiagnosticsConstants.Logs.ExecutionStarted,
        Level = LogLevel.Information,
        Message = "Psyche pipeline execution started. SessionId: {SessionId}, TraceId: {TraceId}")]
    public static partial void ExecutionStarted(this ILogger logger, string sessionId, string traceId);

    [LoggerMessage(
        EventId = VKBehaviorsDiagnosticsConstants.Logs.ExecutionCompleted,
        Level = LogLevel.Information,
        Message = "Psyche pipeline execution completed successfully. TraceId: {TraceId}, Duration: {DurationMs}ms")]
    public static partial void ExecutionCompleted(this ILogger logger, string traceId, double durationMs);

    [LoggerMessage(
        EventId = VKBehaviorsDiagnosticsConstants.Logs.ExecutionFailed,
        Level = LogLevel.Error,
        Message = "Psyche pipeline execution failed. TraceId: {TraceId}, ErrorCode: {ErrorCode}, Message: {ErrorMessage}")]
    public static partial void ExecutionFailed(this ILogger logger, string traceId, string errorCode, string errorMessage);

    [LoggerMessage(
        EventId = VKBehaviorsDiagnosticsConstants.Logs.PipelineStarted,
        Level = LogLevel.Information,
        Message = "Psyche pipeline started. PersonaId: {PersonaId}, SessionId: {SessionId}, CorrelationId: {CorrelationId}")]
    public static partial void PipelineStarted(this ILogger logger, VKPersonaId personaId, VKSessionId sessionId, string correlationId);

    [LoggerMessage(
        EventId = VKBehaviorsDiagnosticsConstants.Logs.PipelineCompleted,
        Level = LogLevel.Information,
        Message = "Psyche pipeline completed successfully. CorrelationId: {CorrelationId}, Duration: {DurationMs}ms")]
    public static partial void PipelineCompleted(this ILogger logger, string correlationId, double durationMs);

    [LoggerMessage(
        EventId = VKBehaviorsDiagnosticsConstants.Logs.PipelineFailed,
        Level = LogLevel.Error,
        Message = "Psyche pipeline failed. CorrelationId: {CorrelationId}, ErrorCode: {ErrorCode}, Message: {ErrorMessage}")]
    public static partial void PipelineFailed(this ILogger logger, string correlationId, string errorCode, string errorMessage);
}
