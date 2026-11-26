using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Service contract for managing token rate limits (TPM) and scope-based token budget allocations.
/// </summary>
public interface IVKAITokenBudgetManager
{
    /// <summary>
    /// Reserves estimated token quota for an upcoming invocation.
    /// </summary>
    /// <param name="scopeKey">The scope key for quota isolation.</param>
    /// <param name="invocationId">The unique invocation identifier.</param>
    /// <param name="estimatedTokens">Estimated input/output token count.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating whether tokens were reserved.</returns>
    Task<VKResult> ReserveTokensAsync(
        VKAIScopeKey scopeKey,
        string invocationId,
        int estimatedTokens,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Settles a prior token reservation with the actual tokens consumed.
    /// </summary>
    /// <param name="scopeKey">The scope key for quota isolation.</param>
    /// <param name="invocationId">The unique invocation identifier.</param>
    /// <param name="actualTokens">Actual token count consumed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<VKResult> SettleTokensAsync(
        VKAIScopeKey scopeKey,
        string invocationId,
        int actualTokens,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a prior token reservation when an invocation is aborted or fails before execution.
    /// </summary>
    /// <param name="scopeKey">The scope key for quota isolation.</param>
    /// <param name="invocationId">The unique invocation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<VKResult> ReleaseTokensAsync(
        VKAIScopeKey scopeKey,
        string invocationId,
        CancellationToken cancellationToken = default);
}
