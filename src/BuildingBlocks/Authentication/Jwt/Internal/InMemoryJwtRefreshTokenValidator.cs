using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Common.Diagnostics.Internal;
using VK.Blocks.Authentication.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Jwt.Internal;

/// <summary>
/// A zero-dependency InMemory implementation of <see cref="IJwtRefreshTokenValidator"/>
/// using <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
internal sealed class InMemoryJwtRefreshTokenValidator(
    IOptions<VKJwtOptions> options,
    TimeProvider timeProvider,
    ILogger<InMemoryJwtRefreshTokenValidator> logger) : IVKJwtRefreshValidator, IInMemoryCacheCleanup, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _consumedJtis = new();
    private readonly ConcurrentDictionary<string, RateLimitState> _refreshRateLimits = new();
    private readonly object _cleanupLock = new();

    /// <inheritdoc />
    public Type AssociatedServiceType => typeof(IVKJwtRefreshValidator);

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _consumedJtis.Clear();
        _refreshRateLimits.Clear();
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<VKResult<bool>> ValidateTokenRotationAsync(string tokenJti, string familyId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tokenJti) || string.IsNullOrWhiteSpace(familyId))
        {
            logger.LogInvalidRefreshTokenRequest();
            return ValueTask.FromResult(VKResult.Failure<bool>(JwtRefreshTokenErrors.InvalidIds));
        }

        // Apply Sliding Window Rate Limiting (Stage 2)
        if (options.Value.EnableRefreshRateLimiting)
        {
            long nowUnix = timeProvider.GetUtcNow().ToUnixTimeSeconds();
            RateLimitState rateLimitState = _refreshRateLimits.GetOrAdd(familyId, _ => new RateLimitState());

            lock (rateLimitState)
            {
                rateLimitState.LastTouchedAt = nowUnix;
                long windowStart = nowUnix - options.Value.RefreshWindowSeconds;

                while (rateLimitState.Timestamps.TryPeek(out long ts) && ts < windowStart)
                {
                    rateLimitState.Timestamps.TryDequeue(out _);
                }

                if (rateLimitState.Timestamps.Count >= options.Value.MaxRefreshAttempts)
                {
                    logger.LogRefreshTokenRateLimitExceeded(familyId);
                    return ValueTask.FromResult(VKResult.Failure<bool>(VKJwtErrors.RateLimitExceeded));
                }

                rateLimitState.Timestamps.Enqueue(nowUnix);
            }
        }

        string cacheKey = $"{familyId}:{tokenJti}";

        if (_consumedJtis.TryGetValue(cacheKey, out DateTimeOffset expiration))
        {
            if (expiration > timeProvider.GetUtcNow())
            {
                // The JTI was already consumed - this is a replay attack!
                logger.LogRefreshTokenReplayDetected(familyId, tokenJti);
                AuthenticationDiagnostics.RecordReplayAttack(familyId);
                return ValueTask.FromResult(VKResult.Failure<bool>(JwtRefreshTokenErrors.Compromised));
            }

            // Lazy cleanup
            _consumedJtis.TryRemove(cacheKey, out _);
        }

        // Cache the consumed JTI.
        int ttlDays = options.Value.RefreshTokenLifetimeDays;
        DateTimeOffset newExpiration = timeProvider.GetUtcNow().AddDays(ttlDays);

        _consumedJtis.AddOrUpdate(cacheKey, newExpiration, (_, _) => newExpiration);

        return ValueTask.FromResult(VKResult.Success(true));
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
            DateTimeOffset now = timeProvider.GetUtcNow();
            long nowUnix = now.ToUnixTimeSeconds();

            // 1. Cleanup expired consumed JTIs
            var expiredKeys = _consumedJtis
                .Where(kvp => kvp.Value < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (string? key in expiredKeys)
            {
                _consumedJtis.TryRemove(key, out _);
            }

            // 2. Cleanup inactive rate limit tracking entries (inactive for > 1 hour)
            var expiredLimitKeys = _refreshRateLimits
                .Where(kvp => kvp.Value.LastTouchedAt < nowUnix - 3600)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (string? key in expiredLimitKeys)
            {
                _refreshRateLimits.TryRemove(key, out _);
            }
        }
        finally
        {
            Monitor.Exit(_cleanupLock);
        }
    }

    private sealed class RateLimitState
    {
        public ConcurrentQueue<long> Timestamps { get; } = new();
        public long LastTouchedAt { get; set; }
    }
}
