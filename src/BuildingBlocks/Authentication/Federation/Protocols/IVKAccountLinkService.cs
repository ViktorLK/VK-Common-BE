using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Federation.Protocols;

/// <summary>
/// Defines a contract for linking external identities to local system user accounts.
/// </summary>
public interface IVKAccountLinkService
{
    /// <summary>
    /// Resolves the internal UserId linked to the specified external provider credentials.
    /// </summary>
    /// <param name="loginProvider">The external provider name (e.g. "Google", "GitHub").</param>
    /// <param name="providerKey">The external provider's unique key for the user.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result containing the UserId if found.</returns>
    ValueTask<VKResult<string>> FindLinkedUserIdAsync(string loginProvider, string providerKey, CancellationToken ct = default);

    /// <summary>
    /// Links an external identity to a local user account.
    /// </summary>
    /// <param name="userId">The internal system UserId.</param>
    /// <param name="loginProvider">The external provider name (e.g. "Google", "GitHub").</param>
    /// <param name="providerKey">The external provider's unique key for the user.</param>
    /// <param name="providerDisplayName">The display name of the external provider profile.</param>
    /// <param name="ct">The cancellation token.</param>
    ValueTask<VKResult> LinkAccountAsync(string userId, string loginProvider, string providerKey, string providerDisplayName, CancellationToken ct = default);

    /// <summary>
    /// Unlinks an external identity from a local user account.
    /// </summary>
    /// <param name="userId">The internal system UserId.</param>
    /// <param name="loginProvider">The external provider name.</param>
    /// <param name="providerKey">The external provider's unique key for the user.</param>
    /// <param name="ct">The cancellation token.</param>
    ValueTask<VKResult> UnlinkAccountAsync(string userId, string loginProvider, string providerKey, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all external identity accounts linked to a local user.
    /// </summary>
    /// <param name="userId">The internal system UserId.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result containing the list of linked accounts.</returns>
    ValueTask<VKResult<IReadOnlyList<VKAccountLinkInfo>>> GetLinkedAccountsAsync(string userId, CancellationToken ct = default);
}
