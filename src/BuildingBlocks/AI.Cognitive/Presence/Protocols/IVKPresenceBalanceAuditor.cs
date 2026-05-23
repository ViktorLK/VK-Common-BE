using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the contract for auditing financial and token credits balance gating.
/// Follows CS.01, CS.03, and AP.03.
/// </summary>
public interface IVKPresenceBalanceAuditor
{
    /// <summary>
    /// Audits the remaining credit or token balances for the specified tenant and user.
    /// </summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="userId">The active user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating whether the user has sufficient balance or credit to perform inference.</returns>
    Task<VKResult> AuditBalanceAsync(
        string tenantId,
        string userId,
        CancellationToken cancellationToken = default); // [CS.03]
}
