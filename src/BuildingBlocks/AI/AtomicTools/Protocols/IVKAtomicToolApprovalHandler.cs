using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Port abstraction for requesting confirmation before executing an atomic tool
/// whose <see cref="VKAtomicToolMetadata.ApprovalMode"/> requires approval.
/// </summary>
public interface IVKAtomicToolApprovalHandler
{
    /// <summary>
    /// Evaluates whether the pending tool invocation is approved to proceed.
    /// </summary>
    /// <param name="invocationContext">The pending atomic tool invocation context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if approved; <c>false</c> or failure if denied.</returns>
    Task<VKResult<bool>> ApproveAsync(
        VKAtomicToolInvocationContext invocationContext,
        CancellationToken cancellationToken = default);
}
