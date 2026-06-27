using System;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.VectorSearch.Common.Diagnostics.Internal;

/// <summary>
/// Centralized logger message source generator definitions for VectorSearch (OR.01).
/// </summary>
internal static partial class VectorSearchLog
{
    [LoggerMessage(EventId = 3001, Level = LogLevel.Information, Message = "VectorSearch pipeline execution started. Query: {Query}")]
    public static partial void PipelineStarted(this ILogger logger, string query);

    [LoggerMessage(EventId = 3002, Level = LogLevel.Error, Message = "VectorSearch pipeline execution failed. Error: {Error}")]
    public static partial void PipelineFailed(this ILogger logger, string error);

    [LoggerMessage(EventId = 3003, Level = LogLevel.Information, Message = "VectorSearch pipeline execution completed. Results Count: {Count}. Elapsed: {Elapsed}ms")]
    public static partial void PipelineCompleted(this ILogger logger, int count, double elapsed);

    [LoggerMessage(EventId = 3004, Level = LogLevel.Information, Message = "Semantic Cache hit. Bypassing search strategy execution.")]
    public static partial void CacheHitBypassed(this ILogger logger);

    [LoggerMessage(EventId = 3005, Level = LogLevel.Warning, Message = "Semantic cache retrieval failed: {Error}")]
    public static partial void CacheRetrievalFailed(this ILogger logger, string error);

    [LoggerMessage(EventId = 3006, Level = LogLevel.Information, Message = "Semantic Cache hit for query: {Query}")]
    public static partial void CacheHit(this ILogger logger, string query);

    [LoggerMessage(EventId = 3007, Level = LogLevel.Warning, Message = "Failed to write to semantic cache: {Error}")]
    public static partial void CacheWriteFailed(this ILogger logger, string error);

    [LoggerMessage(EventId = 3008, Level = LogLevel.Information, Message = "Executing pipeline stage {StageName}.")]
    public static partial void StageExecuting(this ILogger logger, string stageName);

    [LoggerMessage(EventId = 3009, Level = LogLevel.Error, Message = "Pipeline stage {StageName} failed. Error: {Error}")]
    public static partial void StageFailed(this ILogger logger, string stageName, string error);

    [LoggerMessage(EventId = 3010, Level = LogLevel.Information, Message = "Pipeline stage {StageName} completed. Elapsed: {Elapsed}ms")]
    public static partial void StageCompleted(this ILogger logger, string stageName, double elapsed);

    [LoggerMessage(EventId = 3011, Level = LogLevel.Information, Message = "VectorSearch pipeline started. TraceId: {TraceId}")]
    public static partial void PipelineStartedWithTrace(this ILogger logger, string traceId);

    [LoggerMessage(EventId = 3012, Level = LogLevel.Error, Message = "VectorSearch pipeline failed. TraceId: {TraceId}. Error: {Error}")]
    public static partial void PipelineFailedWithTrace(this ILogger logger, string traceId, string error);

    [LoggerMessage(EventId = 3013, Level = LogLevel.Information, Message = "VectorSearch pipeline completed. TraceId: {TraceId}. Elapsed: {Elapsed}ms")]
    public static partial void PipelineCompletedWithTrace(this ILogger logger, string traceId, double elapsed);

    [LoggerMessage(EventId = 3014, Level = LogLevel.Warning, Message = "Search query blocked by SearchGuard. Query: '{Query}'. Reason: {Reason}")]
    public static partial void SearchGuardBlocked(this ILogger logger, string query, string reason);
}
