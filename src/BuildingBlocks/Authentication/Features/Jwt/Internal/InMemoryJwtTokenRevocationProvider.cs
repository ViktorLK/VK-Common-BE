using System.Collections.Concurrent;
using VK.Blocks.Authentication.Common;
using VK.Blocks.Authentication.Features.Jwt.Persistence;

namespace VK.Blocks.Authentication.Features.Jwt.Internal;

/// <summary>
/// A zero-dependency InMemory implementation of <see cref="IJwtTokenRevocationProvider"/>
/// using <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
public sealed class InMemoryJwtTokenRevocationProvider(TimeProvider timeProvider) : IJwtTokenRevocationProvider, IInMemoryCacheCleanup, IAsyncDisposable
{
    #region Fields

    private readonly ConcurrentDictionary<string, DateTimeOffset> _revokedJtis = new();
    private readonly ConcurrentDictionary<string, DateTimeOffset> _revokedUsers = new();
    private readonly object _cleanupLock = new();

    #endregion

    #region Properties

    /// <inheritdoc />
    public Type AssociatedServiceType => typeof(IJwtTokenRevocationProvider);

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _revokedJtis.Clear();
        _revokedUsers.Clear();
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<bool> IsRevokedAsync(string jti, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return ValueTask.FromResult(false);
        }

        if (_revokedJtis.TryGetValue(jti, out var expiration))
        {
            if (expiration > timeProvider.GetUtcNow())
            {
                return ValueTask.FromResult(true);
            }

            // Lazy cleanup
            _revokedJtis.TryRemove(jti, out _);
        }

        return ValueTask.FromResult(false);
    }

    /// <inheritdoc />
    public ValueTask RevokeAsync(string jti, TimeSpan ttl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return ValueTask.CompletedTask;
        }

        var expiration = timeProvider.GetUtcNow().Add(ttl);
        _revokedJtis.AddOrUpdate(jti, expiration, (_, _) => expiration);

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<bool> IsUserRevokedAsync(string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return ValueTask.FromResult(false);
        }

        if (_revokedUsers.TryGetValue(userId, out var expiration))
        {
            if (expiration > timeProvider.GetUtcNow())
            {
                return ValueTask.FromResult(true);
            }

            // Lazy cleanup
            _revokedUsers.TryRemove(userId, out _);
        }

        return ValueTask.FromResult(false);
    }

    /// <inheritdoc />
    public ValueTask RevokeUserAsync(string userId, TimeSpan ttl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return ValueTask.CompletedTask;
        }

        var expiration = timeProvider.GetUtcNow().Add(ttl);
        _revokedUsers.AddOrUpdate(userId, expiration, (_, _) => expiration);

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public void CleanupExpiredEntries()
    {
        if (!Monitor.TryEnter(_cleanupLock))
        {
            return;
        }

        try
        {
            var now = timeProvider.GetUtcNow();

            // Cleanup Revoked JTIs
            var expiredJtis = _revokedJtis
                .Where(kvp => kvp.Value < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredJtis)
            {
                _revokedJtis.TryRemove(key, out _);
            }

            // Cleanup Revoked Users
            var expiredUsers = _revokedUsers
                .Where(kvp => kvp.Value < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredUsers)
            {
                _revokedUsers.TryRemove(key, out _);
            }
        }
        finally
        {
            Monitor.Exit(_cleanupLock);
        }
    }

    #endregion
}
