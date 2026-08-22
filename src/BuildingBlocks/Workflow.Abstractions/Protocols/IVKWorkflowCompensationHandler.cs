using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Business contract for executing compensation / rollback logic for a specific Workflow context.
/// Follows CS.01 and CS.03.
/// </summary>
/// <typeparam name="TContext">The business request/execution context.</typeparam>
public interface IVKWorkflowCompensationHandler<TContext>
{
    /// <summary>
    /// Executes compensation/rollback actions (e.g. unfreezing quotas, refunding balance, marking resources invalid).
    /// </summary>
    Task<VKResult> CompensateAsync(TContext context, VKError originalError, CancellationToken cancellationToken);
}
