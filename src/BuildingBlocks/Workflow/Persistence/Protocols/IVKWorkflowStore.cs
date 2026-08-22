using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Core contract for Workflow instance persistence, state transition CAS updates, and audit history.
/// Follows CS.01 and CS.03.
/// </summary>
public interface IVKWorkflowStore
{
    /// <summary>
    /// Retrieves a Workflow instance by its unique identifier.
    /// </summary>
    Task<VKResult<VKWorkflowInstance>> GetByIdAsync(VKWorkflowId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a Workflow instance by its TraceId / IdempotencyKey and workflow name.
    /// </summary>
    Task<VKResult<VKWorkflowInstance>> GetByTraceIdAsync(string traceId, string workflowName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new Workflow instance record.
    /// </summary>
    Task<VKResult> CreateAsync(VKWorkflowInstance instance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs an atomic Compare-And-Swap (CAS) update on the Workflow instance state and version.
    /// Fails if the current state does not match <paramref name="expectedCurrentState"/> or version mismatch.
    /// </summary>
    Task<VKResult> UpdateAsync(VKWorkflowInstance instance, VKWorkflowState expectedCurrentState, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries orphan Workflow instances that are currently in an in-flight state (<see cref="VKWorkflowState.Processing"/>, <see cref="VKWorkflowState.Compensating"/>, or expired <see cref="VKWorkflowState.Suspended"/>)
    /// and have exceeded their <see cref="VKWorkflowInstance.NextTimeoutAt"/> timestamp.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKWorkflowInstance>>> GetOrphansAsync(DateTimeOffset now, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries child sub-workflow instances spawned by the specified parent workflow.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKWorkflowInstance>>> GetSubWorkflowsAsync(VKWorkflowId parentWorkflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries Workflow instances matching the specified filter criteria with pagination.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKWorkflowInstance>>> QueryAsync(VKWorkflowQueryFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends a new audit history log entry for a state transition.
    /// </summary>
    Task<VKResult> AppendHistoryAsync(VKWorkflowHistoryEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the chronological state transition history for a specific workflow instance.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKWorkflowHistoryEntry>>> GetHistoryAsync(VKWorkflowId id, CancellationToken cancellationToken = default);
}
