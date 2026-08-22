using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Workflow.Observability.Internal;

/// <summary>
/// Fallback no-op alert handler implementation.
/// Follows AP.01.
/// </summary>
internal sealed class DefaultNoOpWorkflowAlertHandler : IVKWorkflowAlertHandler
{
    public Task OnCompensationFailedAsync(VKWorkflowInstance instance, VKError originalError, VKError compensationError, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task OnWorkflowOrphanTimeoutAsync(VKWorkflowInstance instance, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
