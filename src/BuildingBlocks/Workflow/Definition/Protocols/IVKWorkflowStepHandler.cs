using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Business extension contract defining the four-phase Workflow lifecycle steps for a specific execution context.
/// Follows CS.01 and CS.03.
/// </summary>
/// <typeparam name="TContext">The business request/execution context.</typeparam>
/// <typeparam name="TResult">The expected result of the external operation.</typeparam>
public interface IVKWorkflowStepHandler<TContext, TResult>
{
    /// <summary>
    /// Phase 1 (Before): Millisecond-level atomic transaction to record initial state and reserve resources.
    /// </summary>
    Task<VKResult> OnBeforeAsync(TContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Phase 2 (External Execution): Non-transactional, long-running external I/O call (e.g. LLM inference, 3rd-party API).
    /// </summary>
    Task<VKResult<TResult>> ExecuteExternalAsync(TContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Phase 3 (After Success): Millisecond-level atomic transaction to commit result, finalize resource allocation, and mark Completed.
    /// </summary>
    Task<VKResult> OnAfterSuccessAsync(TContext context, TResult result, CancellationToken cancellationToken);

    /// <summary>
    /// Phase 4 (After Failure): Millisecond-level atomic transaction to handle immediate failures, record error, and trigger compensation.
    /// </summary>
    Task<VKResult> OnAfterFailureAsync(TContext context, VKError error, CancellationToken cancellationToken);
}
