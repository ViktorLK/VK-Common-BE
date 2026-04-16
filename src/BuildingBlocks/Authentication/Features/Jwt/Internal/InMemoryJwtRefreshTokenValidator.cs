using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Common;
using VK.Blocks.Authentication.Diagnostics;
using VK.Blocks.Authentication.Features.Jwt.Persistence;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.Features.Jwt.Internal;

/// <summary>
/// A zero-dependency InMemory implementation of <see cref="IJwtRefreshTokenValidator"/>
/// using <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
public sealed class InMemoryJwtRefreshTokenValidator(
    IOptionsMonitor<JwtOptions> options,
    TimeProvider timeProvider,
    ILogger<InMemoryJwtRefreshTokenValidator> logger) : IJwtRefreshTokenValidator, IInMemoryCacheCleanup, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _consumedJtis = new();
    private readonly object _cleanupLock = new();

    /// <inheritdoc />
    public Type AssociatedServiceType => typeof(IJwtRefreshTokenValidator);

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _consumedJtis.Clear();
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<Result<bool>> ValidateTokenRotationAsync(string tokenJti, string familyId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tokenJti) || string.IsNullOrWhiteSpace(familyId))
        {
            logger.LogInvalidRefreshTokenRequest();
            return ValueTask.FromResult(Result.Failure<bool>(JwtRefreshTokenErrors.InvalidIds));
        }

        var cacheKey = $"{familyId}:{tokenJti}";

        if (_consumedJtis.TryGetValue(cacheKey, out var expiration))
        {
            if (expiration > timeProvider.GetUtcNow())
            {
                // The JTI was already consumed - this is a replay attack!
                logger.LogRefreshTokenReplayDetected(familyId, tokenJti);
                AuthenticationDiagnostics.RecordReplayAttack(familyId);
                return ValueTask.FromResult(Result.Failure<bool>(JwtRefreshTokenErrors.Compromised));
            }

            // Lazy cleanup
            _consumedJtis.TryRemove(cacheKey, out _);
        }

        // Cache the consumed JTI.
        var ttlDays = options.CurrentValue.RefreshTokenLifetimeDays;
        var newExpiration = timeProvider.GetUtcNow().AddDays(ttlDays);

        _consumedJtis.AddOrUpdate(cacheKey, newExpiration, (_, _) => newExpiration);

        return ValueTask.FromResult(Result.Success(true));
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
            var expiredKeys = _consumedJtis
                .Where(kvp => kvp.Value < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
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
