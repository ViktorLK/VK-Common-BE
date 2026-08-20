using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.Workflow.Common.Diagnostics.Internal;

[VKBlockDiagnostics<VKWorkflowBlock>]
internal static partial class WorkflowDiagnostics
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Workflow {WorkflowName} with TraceId {TraceId} short-circuited returning cached result.")]
    public static partial void WorkflowShortCircuited(this ILogger logger, string workflowName, string traceId);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Workflow {WorkflowName} with TraceId {TraceId} is already in progress.")]
    public static partial void WorkflowAlreadyInProgress(this ILogger logger, string workflowName, string traceId);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Workflow {WorkflowName} TraceId {TraceId} threw exception during external execution.")]
    public static partial void WorkflowExternalExecutionException(this ILogger logger, string workflowName, string traceId, Exception ex);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Error,
        Message = "Workflow {WorkflowName} TraceId {TraceId} OnAfterSuccess failed: {Error}")]
    public static partial void WorkflowAfterSuccessFailed(this ILogger logger, string workflowName, string traceId, string error);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Warning,
        Message = "Compensation attempt {Attempt}/{Max} returned failure: {Error}")]
    public static partial void CompensationAttemptFailed(this ILogger logger, int attempt, int max, string error);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Error,
        Message = "Compensation attempt {Attempt}/{Max} threw exception.")]
    public static partial void CompensationAttemptException(this ILogger logger, int attempt, int max, Exception ex);

    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Information,
        Message = "Workflow recovery background sweeper service started.")]
    public static partial void RecoveryServiceStarted(this ILogger logger);

    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Warning,
        Message = "Found {Count} orphan Workflow instances exceeding timeout threshold.")]
    public static partial void OrphanWorkflowsFound(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Warning,
        Message = "Orphan Workflow instance {WorkflowName}:{TraceId} marked as TimeoutFailed.")]
    public static partial void OrphanWorkflowMarkedTimeout(this ILogger logger, string workflowName, string traceId);

    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Error,
        Message = "Exception encountered in Workflow recovery scan loop.")]
    public static partial void RecoveryScanLoopException(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Error,
        Message = "Exception encountered in Workflow recovery process loop.")]
    public static partial void RecoveryProcessLoopException(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1012,
        Level = LogLevel.Warning,
        Message = "Workflow {WorkflowName} TraceId {TraceId} OnAfterFailure failed: {Error}")]
    public static partial void WorkflowAfterFailureFailed(this ILogger logger, string workflowName, string traceId, string error);

    [LoggerMessage(
        EventId = 1013,
        Level = LogLevel.Warning,
        Message = "Workflow {WorkflowName} TraceId {TraceId} Step external call failed (Attempt {Attempt}/{MaxRetries}): {Error}. Retrying in {DelayMs}ms.")]
    public static partial void StepRetryScheduled(this ILogger logger, string workflowName, string traceId, int attempt, int maxRetries, string error, double delayMs);

    [LoggerMessage(
        EventId = 1014,
        Level = LogLevel.Warning,
        Message = "Workflow {WorkflowName} TraceId {TraceId} external call failed with non-transient error: {Error}. Aborting retries.")]
    public static partial void NonTransientErrorEncountered(this ILogger logger, string workflowName, string traceId, string error);

    [LoggerMessage(
        EventId = 1015,
        Level = LogLevel.Critical,
        Message = "Workflow {WorkflowName} TraceId {TraceId} entered unrecoverable state CompensationFailed: {Error}")]
    public static partial void UnrecoverableCompensationFailed(this ILogger logger, string workflowName, string traceId, string error);

    [LoggerMessage(
        EventId = 1016,
        Level = LogLevel.Information,
        Message = "Workflow {WorkflowId} suspended with reason: {Reason}. Expiration: {TimeoutAt}")]
    public static partial void WorkflowSuspended(this ILogger logger, string workflowId, string reason, DateTimeOffset timeoutAt);

    [LoggerMessage(
        EventId = 1017,
        Level = LogLevel.Information,
        Message = "Workflow {WorkflowId} successfully resumed.")]
    public static partial void WorkflowResumed(this ILogger logger, string workflowId);

    [LoggerMessage(
        EventId = 1018,
        Level = LogLevel.Information,
        Message = "Sub-workflow {WorkflowName}:{TraceId} started by parent {ParentWorkflowId}:{ParentTraceId}")]
    public static partial void SubWorkflowStarted(this ILogger logger, string workflowName, string traceId, string parentWorkflowId, string parentTraceId);

    [LoggerMessage(
        EventId = 1019,
        Level = LogLevel.Warning,
        Message = "Workflow {WorkflowName} TraceId {TraceId} rejected: Circuit breaker is OPEN for key {CircuitBreakerKey}.")]
    public static partial void StepCircuitBreakerOpen(this ILogger logger, string workflowName, string traceId, string circuitBreakerKey);

    [LoggerMessage(
        EventId = 1020,
        Level = LogLevel.Warning,
        Message = "Workflow {WorkflowName} TraceId {TraceId} timed out after {TimeoutSeconds}s.")]
    public static partial void StepTimeoutExceeded(this ILogger logger, string workflowName, string traceId, double timeoutSeconds);
}
