using System;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Authentication.Abstractions;

/// <summary>
/// Manages a deny-list of API Keys that have been explicitly revoked.
/// </summary>
public interface IApiKeyBlacklist
{
    /// <summary>
    /// Returns <c>true</c> when the given API Key ID has been added to the deny-list.
    /// </summary>
    /// <param name="keyId">The API Key ID to check.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous check operation.</returns>
    Task<bool> IsRevokedAsync(string keyId, CancellationToken ct = default);

    /// <summary>
    /// Adds <paramref name="keyId"/> to the deny-list for the specified <paramref name="ttl"/>.
    /// </summary>
    /// <param name="keyId">The API Key ID to revoke.</param>
    /// <param name="ttl">The time-to-live for the revocation entry.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous revocation operation.</returns>
    Task RevokeAsync(string keyId, TimeSpan ttl, CancellationToken ct = default);
}
