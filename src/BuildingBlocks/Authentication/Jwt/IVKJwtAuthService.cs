using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Defines a service for authenticating users using JWT tokens.
/// </summary>
public interface IVKJwtAuthService
{
    /// <summary>
    /// Authenticates a user based on the provided token.
    /// </summary>
    /// <param name="token">The authentication token.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A task that represents the asynchronous authentication operation. The task result contains the authentication result.</returns>
    ValueTask<VKResult<VKAuthenticatedUser>> AuthenticateAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether the provided <see cref="ClaimsPrincipal"/> or its token (jti) has been revoked.
    /// </summary>
    /// <param name="principal">The principal to check.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result indicating whether the principal is still valid.</returns>
    Task<VKResult> ValidateRevocationAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}
