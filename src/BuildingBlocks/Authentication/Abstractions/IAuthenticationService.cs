using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Authentication.Abstractions.Contracts;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.Abstractions;

/// <summary>
/// Defines a service for authenticating users.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user based on the provided token.
    /// </summary>
    /// <param name="token">The authentication token.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A task that represents the asynchronous authentication operation. The task result contains the authentication result.</returns>
    Task<Result<AuthUser>> AuthenticateAsync(string token, CancellationToken cancellationToken = default);
}
