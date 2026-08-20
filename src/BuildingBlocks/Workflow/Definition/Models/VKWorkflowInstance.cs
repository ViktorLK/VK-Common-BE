using System;

namespace VK.Blocks.Workflow;

/// <summary>
/// Runtime state persistence model of a Workflow instance.
/// Follows AP.01.
/// </summary>
public sealed record VKWorkflowInstance
{
    /// <summary>
    /// Unique identifier of the Workflow instance.
    /// </summary>
    public required VKWorkflowId Id { get; init; }

    /// <summary>
    /// TraceId / IdempotencyKey associated with this execution.
    /// </summary>
    public required string TraceId { get; init; }

    /// <summary>
    /// Correlation identifier spanning across distributed systems.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Name or type descriptor of the Workflow.
    /// </summary>
    public required string WorkflowName { get; init; }

    /// <summary>
    /// Schema or definition version of the Workflow (defaults to 1).
    /// </summary>
    public int DefinitionVersion { get; init; } = 1;

    /// <summary>
    /// Identifier of the parent workflow instance if this is a sub-workflow.
    /// </summary>
    public VKWorkflowId? ParentWorkflowId { get; init; }

    /// <summary>
    /// TraceId of the parent workflow if this is a sub-workflow.
    /// </summary>
    public string? ParentTraceId { get; init; }

    /// <summary>
    /// Current lifecycle state.
    /// </summary>
    public VKWorkflowState CurrentState { get; init; } = VKWorkflowState.Pending;

    /// <summary>
    /// Reason or metadata if the workflow is currently in Suspended state.
    /// </summary>
    public string? SuspendReason { get; init; }

    /// <summary>
    /// Serialized context / payload snapshot.
    /// </summary>
    public string? PayloadJson { get; init; }

    /// <summary>
    /// Serialized result if completed.
    /// </summary>
    public string? ResultJson { get; init; }

    /// <summary>
    /// Last error description if failed.
    /// </summary>
    public string? LastError { get; init; }

    /// <summary>
    /// Number of retry attempts executed.
    /// </summary>
    public int RetryCount { get; init; }

    /// <summary>
    /// Optimistic locking concurrency token/version.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Timestamp when this instance was initiated (UTC).
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Timestamp when this instance was last updated (UTC).
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; }

    /// <summary>
    /// Expiration timestamp when this in-flight instance is considered an orphan.
    /// </summary>
    public DateTimeOffset NextTimeoutAt { get; init; }
}
