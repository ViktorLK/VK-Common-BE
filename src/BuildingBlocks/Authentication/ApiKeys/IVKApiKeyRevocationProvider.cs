using System;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Authentication;

/// <summary>
/// Manages revocation status for API Keys.
/// </summary>
public interface IVKApiKeyRevocationProvider
{
    /// <summary>
    /// Returns <c>true</c> when the given API Key ID has been revoked.
    /// </summary>
    /// <param name="keyId">The API Key ID to check.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous check operation.</returns>
    ValueTask<bool> IsRevokedAsync(string keyId, CancellationToken ct = default);

    /// <summary>
    /// Marks <paramref name="keyId"/> as revoked for the specified <paramref name="ttl"/>.
    /// </summary>
    /// <param name="keyId">The API Key ID to revoke.</param>
    /// <param name="ttl">The time-to-live for the revocation entry.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous revocation operation.</returns>
    ValueTask RevokeAsync(string keyId, TimeSpan ttl, CancellationToken ct = default);
}




