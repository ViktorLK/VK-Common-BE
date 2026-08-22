using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Extensibility contract for handling alert notifications when workflow instances enter unrecoverable failure or orphan timeout states.
/// </summary>
public interface IVKWorkflowAlertHandler
{
    /// <summary>
    /// Triggered when compensation rollback permanently fails after all retry attempts.
    /// </summary>
    Task OnCompensationFailedAsync(VKWorkflowInstance instance, VKError originalError, VKError compensationError, CancellationToken cancellationToken);

    /// <summary>
    /// Triggered when an orphan in-flight workflow instance exceeds timeout threshold and is harvested by the recovery sweeper.
    /// </summary>
    Task OnWorkflowOrphanTimeoutAsync(VKWorkflowInstance instance, CancellationToken cancellationToken);
}
