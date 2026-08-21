using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Domain contract to orchestrate iterative, multi-step Agentic loops (e.g. ReAct, Reflexion, Step-by-Step Problem Solving).
/// Follows [CS.01], [CS.03], and [AP.01].
/// </summary>
public interface IVKAgentLoopOrchestrator
{
    /// <summary>
    /// Executes an iterative loop using the underlying <see cref="IVKChatTurnOrchestrator"/> until the exit condition is met or the iteration limit is reached.
    /// </summary>
    /// <param name="request">The initial loop request parameters.</param>
    /// <param name="exitCondition">Predicate evaluated after each iteration step. Return <c>true</c> to exit the loop; <c>false</c> to continue.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the aggregated <see cref="VKAgentLoopResult"/>.</returns>
    Task<VKResult<VKAgentLoopResult>> RunLoopAsync(
        VKAgentLoopRequest request,
        Func<VKChatTurnResult, bool>? exitCondition = null,
        CancellationToken cancellationToken = default);
}
