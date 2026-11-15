using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Extension point enabling upper orchestration layers (e.g. Cortex, Psyche, Engram bridges)
/// to inject system instruction appendices, retrieved memory messages, or context variables
/// without coupling the agent runtime to concrete prompt or memory libraries (ARCH-01, ARCH-02).
/// </summary>
public interface IVKAgentContextProvider
{
    /// <summary>
    /// Gets the execution order when multiple providers are registered. Lower values run first.
    /// </summary>
    int Order => 0;

    /// <summary>
    /// Supplies supplemental runtime context for an upcoming agent invocation.
    /// </summary>
    /// <param name="request">Metadata describing the active agent invocation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Supplemental context or failure.</returns>
    Task<VKResult<VKAgentAdditionalContext>> ProvideContextAsync(
        VKAgentContextRequest request,
        CancellationToken cancellationToken = default);
}
