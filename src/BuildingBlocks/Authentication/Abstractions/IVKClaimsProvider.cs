using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Authentication;

/// <summary>
/// Defines a provider for retrieving user claims dynamically (e.g., from a database).
/// This interface should be implemented by the application to provide custom claims during authentication.
/// </summary>
public interface IVKClaimsProvider
{
    /// <summary>
    /// Retrieves claims for the specified user asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of claims to append to the authenticated user's identity.</returns>
    ValueTask<IEnumerable<Claim>> GetUserClaimsAsync(string userId, CancellationToken cancellationToken = default);
}




