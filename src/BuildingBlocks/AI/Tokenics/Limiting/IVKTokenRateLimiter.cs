using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the contract for an AI Token Rate Limiter.
/// Implements TPM (Tokens Per Minute) and RPM (Requests Per Minute) limiting.
/// </summary>
public interface IVKTokenRateLimiter
{
    /// <summary>
    /// Acquires permission to execute an operation with the estimated token cost.
    /// </summary>
    /// <param name="estimatedTokens">The estimated tokens for the request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating whether the request is allowed.</returns>
    Task<VKResult> AcquireAsync(int estimatedTokens, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports actual token usage after an operation completes to refine the rate limit state.
    /// </summary>
    /// <param name="actualTokens">The actual tokens consumed.</param>
    /// <returns>A task representing the operation.</returns>
    Task ReportUsageAsync(int actualTokens);
}
