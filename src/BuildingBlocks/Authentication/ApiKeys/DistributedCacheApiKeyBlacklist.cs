using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using VK.Blocks.Authentication.Abstractions;

namespace VK.Blocks.Authentication.ApiKeys;

/// <summary>
/// Implements <see cref="IApiKeyBlacklist"/> using an <see cref="IDistributedCache"/> to store revoked API Keys.
/// </summary>
public sealed class DistributedCacheApiKeyBlacklist(IDistributedCache cache) : IApiKeyBlacklist
{
    private const string KeyPrefix = "revoked_apikey:";

    /// <inheritdoc />
    public async Task<bool> IsRevokedAsync(string keyId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(keyId))
        {
            return false;
        }

        var key = $"{KeyPrefix}{keyId}";
        var value = await cache.GetStringAsync(key, ct).ConfigureAwait(false);

        return value != null;
    }

    /// <inheritdoc />
    public async Task RevokeAsync(string keyId, TimeSpan ttl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(keyId))
        {
            return;
        }

        var key = $"{KeyPrefix}{keyId}";
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        // Store a simple placeholder value "1"
        await cache.SetStringAsync(key, "1", options, ct).ConfigureAwait(false);
    }
}
