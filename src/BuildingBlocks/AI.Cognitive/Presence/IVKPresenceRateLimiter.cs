using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the contract for auditing and enforcing high-frequency request rate limits per tenant and user.
/// Follows CS.01, CS.03, and AP.03.
/// </summary>
public interface IVKPresenceRateLimiter
{
    /// <summary>
    /// Audits the request frequency for the specified tenant and user.
    /// </summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="userId">The active user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating whether the request is allowed or has exceeded the limit.</returns>
    Task<VKResult> AuditRateLimitAsync(
        string tenantId,
        string userId,
        CancellationToken cancellationToken = default); // [CS.03]
}
