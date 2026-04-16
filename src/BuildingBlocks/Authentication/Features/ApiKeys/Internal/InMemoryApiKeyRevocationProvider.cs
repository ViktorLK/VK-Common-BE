using System.Collections.Concurrent;
using VK.Blocks.Authentication.Common;
using VK.Blocks.Authentication.Features.ApiKeys.Persistence;

namespace VK.Blocks.Authentication.Features.ApiKeys.Internal;

/// <summary>
/// A high-performance, zero-dependency InMemory implementation of <see cref="IApiKeyRevocationProvider"/>
/// using <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
public sealed class InMemoryApiKeyRevocationProvider(TimeProvider timeProvider) : IApiKeyRevocationProvider, IInMemoryCacheCleanup, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _revocations = new();
    private readonly object _cleanupLock = new();

    /// <inheritdoc />
    public Type AssociatedServiceType => typeof(IApiKeyRevocationProvider);

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _revocations.Clear();
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<bool> IsRevokedAsync(string keyId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(keyId))
        {
            return ValueTask.FromResult(false);
        }

        if (_revocations.TryGetValue(keyId, out var expiration))
        {
            if (expiration > timeProvider.GetUtcNow())
            {
                return ValueTask.FromResult(true);
            }

            // Lazy cleanup of expired entry
            _revocations.TryRemove(keyId, out _);
        }

        return ValueTask.FromResult(false);
    }

    /// <inheritdoc />
    public ValueTask RevokeAsync(string keyId, TimeSpan ttl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(keyId))
        {
            return ValueTask.CompletedTask;
        }

        var expiration = timeProvider.GetUtcNow().Add(ttl);
        _revocations.AddOrUpdate(keyId, expiration, (_, _) => expiration);

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
            var expiredKeys = _revocations
                .Where(kvp => kvp.Value < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _revocations.TryRemove(key, out _);
            }
        }
        finally
        {
            Monitor.Exit(_cleanupLock);
        }
    }
}
