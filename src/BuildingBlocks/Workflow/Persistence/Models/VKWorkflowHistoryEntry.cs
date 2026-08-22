using System;

namespace VK.Blocks.Workflow;

/// <summary>
/// Audit trail history record representing a single state transition of a Workflow instance.
/// Follows AP.01.
/// </summary>
public sealed record VKWorkflowHistoryEntry
{
    /// <summary>
    /// Unique identifier of the history log record.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Identifier of the workflow instance.
    /// </summary>
    public required VKWorkflowId WorkflowId { get; init; }

    /// <summary>
    /// TraceId / IdempotencyKey of the workflow.
    /// </summary>
    public required string TraceId { get; init; }

    /// <summary>
    /// The source state before this transition.
    /// </summary>
    public required VKWorkflowState FromState { get; init; }

    /// <summary>
    /// The target state after this transition.
    /// </summary>
    public required VKWorkflowState ToState { get; init; }

    /// <summary>
    /// Action, phase or component triggering the transition (e.g. Orchestrator.Before, Orchestrator.AfterSuccess, RecoverySweeper).
    /// </summary>
    public required string Trigger { get; init; }

    /// <summary>
    /// Error description if this transition was triggered by a failure.
    /// </summary>
    public string? ErrorDescription { get; init; }

    /// <summary>
    /// Serialized context snapshot or additional metadata.
    /// </summary>
    public string? DetailsJson { get; init; }

    /// <summary>
    /// Timestamp when this transition took place (UTC).
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }
}
