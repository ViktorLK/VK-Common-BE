using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Service contract for managing token rate limits (TPM) and tenant/provider token budget allocations.
/// </summary>
public interface IVKAITokenBudgetManager
{
    /// <summary>
    /// Checks and acquires token quota for an upcoming operation.
    /// </summary>
    /// <param name="tenantOrKey">Tenant identifier or Provider key.</param>
    /// <param name="estimatedTokens">Estimated input/output token count.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating whether tokens were allocated.</returns>
    Task<VKResult> AcquireTokensAsync(string tenantOrKey, int estimatedTokens, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records actual tokens consumed after operation completion to adjust sliding window counters.
    /// </summary>
    /// <param name="tenantOrKey">Tenant identifier or Provider key.</param>
    /// <param name="actualTokens">Actual token count consumed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<VKResult> RecordUsageAsync(string tenantOrKey, int actualTokens, CancellationToken cancellationToken = default);
}
