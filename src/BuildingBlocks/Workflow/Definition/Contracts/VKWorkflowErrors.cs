using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Domain and operational errors for the Workflow building block.
/// Follows CS.01.
/// </summary>
public static class VKWorkflowErrors
{
    public static readonly VKError DuplicateTraceId =
        VKError.Conflict("Workflow.DuplicateTraceId", "A workflow instance with the same TraceId is already executing or completed.");

    public static readonly VKError ConcurrentExecutionConflict =
        VKError.Conflict("Workflow.ConcurrentExecutionConflict", "Concurrent worker conflict detected during state transition optimistic locking.");

    public static readonly VKError NotFound =
        VKError.NotFound("Workflow.NotFound", "The requested workflow instance was not found.");

    public static readonly VKError InvalidStateTransition =
        VKError.Validation("Workflow.InvalidStateTransition", "The requested workflow state transition is not permitted by the state machine.");

    public static readonly VKError ExternalExecutionFailed =
        VKError.Failure("Workflow.ExternalExecutionFailed", "An error occurred during external non-transactional execution.");

    public static readonly VKError CompensationFailed =
        VKError.Failure("Workflow.CompensationFailed", "Compensation rollback failed after maximum retry attempts.");

    public static readonly VKError OrphanTimeout =
        VKError.Failure("Workflow.OrphanTimeout", "Workflow execution exceeded timeout threshold and was terminated by the recovery sweeper.");

    public static readonly VKError CircuitBreakerOpen =
        VKError.Failure("Workflow.CircuitBreakerOpen", "Execution rejected because the circuit breaker is currently open for the specified key.");

    public static readonly VKError StepTimeout =
        VKError.Failure("Workflow.StepTimeout", "External step execution exceeded the configured timeout limit.");
}
