using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Common.Internal;
using VK.Blocks.Authentication.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Jwt.Internal;

/// <summary>
/// A zero-dependency InMemory implementation of <see cref="IJwtRefreshTokenValidator"/>
/// using <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
internal sealed class InMemoryJwtRefreshTokenValidator(
    IOptionsMonitor<VKJwtOptions> options,
    TimeProvider timeProvider,
    ILogger<InMemoryJwtRefreshTokenValidator> logger) : IVKJwtRefreshValidator, IInMemoryCacheCleanup, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _consumedJtis = new();
    private readonly object _cleanupLock = new();

    /// <inheritdoc />
    public Type AssociatedServiceType => typeof(IVKJwtRefreshValidator);

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _consumedJtis.Clear();
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
        int ttlDays = options.CurrentValue.RefreshTokenLifetimeDays;
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
            var expiredKeys = _consumedJtis
                .Where(kvp => kvp.Value < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (string? key in expiredKeys)
            {
                _consumedJtis.TryRemove(key, out _);
            }
        }
        finally
        {
            Monitor.Exit(_cleanupLock);
        }
    }
}








