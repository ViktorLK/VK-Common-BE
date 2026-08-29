using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pipeline.Diagnostics.Internal;

/// <summary>
/// Source-generated logger messages and metrics for Psyche Pipeline execution.
/// Follows BB.04 and OR.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class PipelineDiagnostics
{
    // --- Source Generated Metrics (v1.5) ---

    [VKMetricHistogram(
        VKPipelineDiagnosticsConstants.Metrics.PipelineDuration,
        Unit = "ms",
        Description = "Measures the duration of Psyche prompt weaving pipeline execution in milliseconds.")]
    public static partial void RecordPipelineExecution(
        double durationMs,
        [VKMetricTag(VKPipelineDiagnosticsConstants.Tags.IsSuccess)] bool success);

    [VKMetricHistogram(
        VKPipelineDiagnosticsConstants.Metrics.StageDuration,
        Unit = "ms",
        Description = "Measures the duration of individual Psyche pipeline stage execution in milliseconds.")]
    public static partial void RecordStageExecution(
        double durationMs,
        [VKMetricTag(VKPipelineDiagnosticsConstants.Tags.StageName)] string stageName,
        [VKMetricTag(VKPipelineDiagnosticsConstants.Tags.IsSuccess)] bool success);

    [VKMetricHistogram(
        VKPipelineDiagnosticsConstants.Metrics.LLMInvocationDuration,
        Unit = "ms",
        Description = "Measures the duration of terminal LLM chat engine invocation in milliseconds.")]
    public static partial void RecordLLMInvocation(
        double durationMs,
        [VKMetricTag(VKPipelineDiagnosticsConstants.Tags.Model)] string model,
        [VKMetricTag(VKPipelineDiagnosticsConstants.Tags.IsSuccess)] bool success);

    // --- [LoggerMessage] Generators (OR.01) ---

    [LoggerMessage(
        EventId = VKPipelineDiagnosticsConstants.Logs.ExecutionStarted,
        Level = LogLevel.Information,
        Message = "Psyche pipeline execution started. SessionId: {SessionId}, CorrelationId: {CorrelationId}")]
    public static partial void ExecutionStarted(this ILogger logger, string sessionId, string correlationId);

    [LoggerMessage(
        EventId = VKPipelineDiagnosticsConstants.Logs.ExecutionCompleted,
        Level = LogLevel.Information,
        Message = "Psyche pipeline execution completed successfully. CorrelationId: {CorrelationId}, Duration: {DurationMs}ms")]
    public static partial void ExecutionCompleted(this ILogger logger, string correlationId, double durationMs);

    [LoggerMessage(
        EventId = VKPipelineDiagnosticsConstants.Logs.ExecutionFailed,
        Level = LogLevel.Error,
        Message = "Psyche pipeline execution failed. CorrelationId: {CorrelationId}, ErrorCode: {ErrorCode}, Message: {ErrorMessage}")]
    public static partial void ExecutionFailed(this ILogger logger, string correlationId, string errorCode, string errorMessage);

    [LoggerMessage(
        EventId = VKPipelineDiagnosticsConstants.Logs.PipelineStarted,
        Level = LogLevel.Information,
        Message = "Psyche pipeline started. PersonaIds: {PersonaIds}, SessionId: {SessionId}, CorrelationId: {CorrelationId}")]
    public static partial void PipelineStarted(this ILogger logger, string personaIds, VKSessionId sessionId, string correlationId);

    [LoggerMessage(
        EventId = VKPipelineDiagnosticsConstants.Logs.PipelineCompleted,
        Level = LogLevel.Information,
        Message = "Psyche pipeline completed successfully. CorrelationId: {CorrelationId}, Duration: {DurationMs}ms")]
    public static partial void PipelineCompleted(this ILogger logger, string correlationId, double durationMs);

    [LoggerMessage(
        EventId = VKPipelineDiagnosticsConstants.Logs.PipelineFailed,
        Level = LogLevel.Error,
        Message = "Psyche pipeline failed. CorrelationId: {CorrelationId}, ErrorCode: {ErrorCode}, Message: {ErrorMessage}")]
    public static partial void PipelineFailed(this ILogger logger, string correlationId, string errorCode, string errorMessage);
}
