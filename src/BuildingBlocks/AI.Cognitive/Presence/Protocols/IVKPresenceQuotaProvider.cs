using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the contract for dynamically resolving token limits and margins per tenant and user.
/// Follows CS.01, CS.03, and AP.03.
/// </summary>
public interface IVKPresenceQuotaProvider
{
    /// <summary>
    /// Resolves the presence token quota configuration for the specified tenant and user.
    /// </summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="userId">The active user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the resolved token quota.</returns>
    Task<VKResult<VKPresenceQuota>> GetQuotaAsync(
        string tenantId,
        string userId,
        CancellationToken cancellationToken = default); // [CS.03]
}
