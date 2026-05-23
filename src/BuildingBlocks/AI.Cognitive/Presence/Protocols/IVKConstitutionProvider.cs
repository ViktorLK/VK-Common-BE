using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the contract for dynamically resolving the L1 Constitution prompt per tenant and user.
/// Follows CS.01, CS.03, and AP.03.
/// </summary>
public interface IVKConstitutionProvider
{
    /// <summary>
    /// Gets the custom constitution segment based on tenant and user context.
    /// </summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="userId">The active user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the custom constitution markdown overlay.</returns>
    Task<VKResult<string>> GetConstitutionAsync(
        string tenantId,
        string userId,
        CancellationToken cancellationToken = default); // [CS.03]
}
