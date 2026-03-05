using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using VK.Blocks.Authentication.Abstractions;

namespace VK.Blocks.Authentication.Security;

/// <summary>
/// Implements <see cref="ITokenBlacklist"/> using an <see cref="IDistributedCache"/> to store revoked JTI claims.
/// </summary>
public sealed class DistributedCacheTokenBlacklist(IDistributedCache cache) : ITokenBlacklist
{
    private const string KeyPrefix = "revoked_jti:";
    private const string UserKeyPrefix = "revoked_user:";

    /// <inheritdoc />
    public async Task<bool> IsRevokedAsync(string jti, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return false;
        }

        var key = $"{KeyPrefix}{jti}";
        var value = await cache.GetStringAsync(key, ct).ConfigureAwait(false);

        return value != null;
    }

    /// <inheritdoc />
    public async Task RevokeAsync(string jti, TimeSpan ttl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return;
        }

        var key = $"{KeyPrefix}{jti}";
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        // Store a simple placeholder value "1"
        await cache.SetStringAsync(key, "1", options, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> IsUserRevokedAsync(string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var key = $"{UserKeyPrefix}{userId}";
        var value = await cache.GetStringAsync(key, ct).ConfigureAwait(false);

        return value != null;
    }

    /// <inheritdoc />
    public async Task RevokeUserAsync(string userId, TimeSpan ttl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var key = $"{UserKeyPrefix}{userId}";
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        // Store a simple placeholder value "1"
        await cache.SetStringAsync(key, "1", options, ct).ConfigureAwait(false);
    }
}
