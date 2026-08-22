using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Extension contract for handling asynchronous event resumption in long-running suspendable workflows.
/// Follows CS.01 and CS.03.
/// </summary>
/// <typeparam name="TContext">The business request/execution context.</typeparam>
/// <typeparam name="TResumePayload">The external event / callback payload delivered upon resumption.</typeparam>
public interface IVKWorkflowResumeHandler<TContext, in TResumePayload>
{
    /// <summary>
    /// Merges the external resume payload into the execution context and validates resumption criteria before continuing.
    /// </summary>
    Task<VKResult> OnResumeAsync(TContext context, TResumePayload resumePayload, CancellationToken cancellationToken);
}
