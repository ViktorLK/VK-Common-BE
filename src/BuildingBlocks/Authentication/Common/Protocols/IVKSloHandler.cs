using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Common.Protocols;

/// <summary>
/// Defines a contract for coordinating Single Sign-Out (SLO) globally across all authentication schemes.
/// </summary>
public interface IVKSloHandler
{
    /// <summary>
    /// Signs out the specified user globally by invalidating active Cookie sessions and revoking JWT refresh tokens.
    /// </summary>
    /// <param name="userId">The unique user identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result indicating success or failure of the sign-out operation.</returns>
    ValueTask<VKResult> SignOutUserGloballyAsync(string userId, CancellationToken ct = default);
}
