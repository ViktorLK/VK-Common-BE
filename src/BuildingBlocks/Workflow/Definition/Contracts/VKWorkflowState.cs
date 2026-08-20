namespace VK.Blocks.Workflow;

/// <summary>
/// Defines the lifecycle states of a Workflow instance.
/// </summary>
public enum VKWorkflowState
{
    /// <summary>
    /// Initial state: instance has been created and prepared for execution.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// In-flight state: before transaction committed and external call is currently in progress.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Success terminal state: external call succeeded and after-success transaction committed.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Compensation state: external call failed or timed out and rollback actions are in progress.
    /// </summary>
    Compensating = 3,

    /// <summary>
    /// Clean failure terminal state: external call failed and compensation succeeded.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Unrecoverable failure terminal state: compensation actions failed after max retries.
    /// </summary>
    CompensationFailed = 5,

    /// <summary>
    /// Timeout terminal state: execution exceeded max timeout threshold and was harvested by background recovery sweeper.
    /// </summary>
    TimeoutFailed = 6,

    /// <summary>
    /// Suspended state: workflow is paused waiting for an asynchronous external event or human approval.
    /// </summary>
    Suspended = 7
}
