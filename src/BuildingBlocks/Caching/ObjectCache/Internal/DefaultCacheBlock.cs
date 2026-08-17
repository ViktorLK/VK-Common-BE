using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using VK.Blocks.Caching.Resilience.Avalanche;
using VK.Blocks.Caching.Resilience.Breakdown;
using VK.Blocks.Caching.Resilience.Penetration;
using VK.Blocks.Core;

namespace VK.Blocks.Caching.ObjectCache.Internal;

/// <summary>
/// Core implementation of IVKCacheBlock that orchestrates providers, serialization, and resilience.
/// </summary>
internal sealed class DefaultCacheBlock(
    ICacheProvider provider,
    ICacheSerializer serializer,
    IVKDistributedLockProvider lockProvider,
    ICacheKeyBuilder keyBuilder,
    IJitterExpiryStrategy jitterStrategy,
    ILockBasedGuard breakdownGuard,
    INullValueGuard penetrationGuard,
    IOptions<VKCachingOptions> cachingOptions,
    TimeProvider timeProvider,
    ILogger<DefaultCacheBlock> logger) : IVKCacheBlock
{
    private readonly VKCachingOptions _cachingOptions = cachingOptions.Value;
    private readonly VKResilienceOptions _resilienceOptions = cachingOptions.Value.Resilience;

    /// <inheritdoc />
    public async Task<VKResult<VKCacheValue<T>>> GetAsync<T>(string key, CancellationToken ct = default)
    {
        using var activity = CachingDiagnostics.StartActivity("Cache.Get");
        var fullKey = keyBuilder.BuildKey(key);
        activity?.SetTag("cache.key", fullKey);

        try
        {
            var providerResult = await provider.GetAsync(fullKey, ct).ConfigureAwait(false);
            if (providerResult.IsFailure)
            {
                CachingDiagnostics.RecordError("get");
                return VKResult.Failure<VKCacheValue<T>>(providerResult.Errors);
            }

            var bytes = providerResult.Value;
            if (bytes is null)
            {
                CachingDiagnostics.RecordMiss();
                return VKResult.Success(VKCacheValue<T>.NoValue);
            }

            var entry = serializer.Deserialize<CacheEntry<T>>(bytes);
            if (entry is null || entry.Version != 1)
            {
                CachingDiagnostics.RecordMiss();
                return VKResult.Success(VKCacheValue<T>.NoValue);
            }

            if (entry.IsExpired(timeProvider.GetUtcNow()))
            {
                await provider.RemoveAsync(fullKey, ct).ConfigureAwait(false);
                CachingDiagnostics.RecordMiss();
                return VKResult.Success(VKCacheValue<T>.NoValue);
            }

            CachingDiagnostics.RecordHit();
            return VKResult.Success(VKCacheValue<T>.ValueOf(penetrationGuard.Unwrap<T>(entry.Value)));
        }
        catch (Exception ex)
        {
            CachingDiagnostics.RecordError("get");
            logger.LogError(ex, "Cache get error for key: {Key}", fullKey);
            return VKResult.Failure<VKCacheValue<T>>(VKCachingErrors.ProviderError);
        }
    }

    /// <inheritdoc />
    public async Task<VKResult> SetAsync<T>(string key, T value, VKCacheOptions? options = null, CancellationToken ct = default)
    {
        using var activity = CachingDiagnostics.StartActivity("Cache.Set");
        var fullKey = keyBuilder.BuildKey(key);
        options ??= VKCacheOptions.Default;
        activity?.SetTag("cache.key", fullKey);

        try
        {
            var expiration = options.Expiration ?? _cachingOptions.DefaultExpiration;

            if (_resilienceOptions.EnableAvalancheProtection)
            {
                expiration = jitterStrategy.ApplyJitter(expiration, _resilienceOptions.MaxJitterRatio);
            }

            var wrappedValue = penetrationGuard.Wrap(value, _resilienceOptions.EnablePenetrationProtection);
            var entry = new CacheEntry<object>(
                wrappedValue,
                timeProvider.GetUtcNow().Add(expiration),
                options.SlidingExpiration);

            var bytes = serializer.Serialize(entry);
            var providerResult = await provider.SetAsync(fullKey, bytes, options, ct).ConfigureAwait(false);

            return providerResult;
        }
        catch (Exception ex)
        {
            CachingDiagnostics.RecordError("set");
            logger.LogError(ex, "Cache set error for key: {Key}", fullKey);

            if (!options.SuppressErrors)
                return VKResult.Failure(VKCachingErrors.ProviderError);

            return VKResult.Success();
        }
    }

    /// <inheritdoc />
    public async Task<VKResult<T>> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        VKCacheOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= VKCacheOptions.Default;
        var fullKey = keyBuilder.BuildKey(key);

        if (!options.BypassCache)
        {
            var cachedResult = await GetAsync<T>(key, ct).ConfigureAwait(false);
            if (cachedResult.IsSuccess && cachedResult.Value!.HasValue)
            {
                var value = cachedResult.Value.Value;
                if (value is not null)
                    return VKResult.Success(value);
            }
        }

        if (_resilienceOptions.EnableBreakdownProtection)
        {
            var breakdownResult = await breakdownGuard.ExecuteAsync<T>(
                lockProvider,
                key,
                async (c) =>
                {
                    var res = await factory(c).ConfigureAwait(false);
                    await SetAsync(key, res, options, c).ConfigureAwait(false);
                    return VKResult.Success(res);
                },
                () => GetAsync<T>(key, ct),
                ct).ConfigureAwait(false);

            if (breakdownResult.IsSuccess && breakdownResult.Value!.HasValue)
            {
                var value = breakdownResult.Value.Value;
                if (value is not null)
                    return VKResult.Success(value);
            }
        }

        var result = await factory(ct).ConfigureAwait(false);
        await SetAsync(key, result, options, ct).ConfigureAwait(false);
        return VKResult.Success(result);
    }

    /// <inheritdoc />
    public async Task<VKResult> RemoveAsync(string key, CancellationToken ct = default)
    {
        using var activity = CachingDiagnostics.StartActivity("Cache.Remove");
        var fullKey = keyBuilder.BuildKey(key);
        activity?.SetTag("cache.key", fullKey);

        return await provider.RemoveAsync(fullKey, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<VKResult> RemoveByTagAsync(string tag, CancellationToken ct = default)
    {
        using var activity = CachingDiagnostics.StartActivity("Cache.RemoveByTag");
        activity?.SetTag("cache.tag", tag);

        return await provider.RemoveByTagAsync(tag, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<VKResult<IVKDistributedLock>> AcquireLockAsync(string resourceKey, VKLockOptions? options = null, CancellationToken ct = default)
    {
        using var activity = CachingDiagnostics.StartActivity("Cache.AcquireLock");
        options ??= new VKLockOptions();
        var fullKey = keyBuilder.BuildKey(resourceKey);
        activity?.SetTag("lock.key", fullKey);

        var @lock = lockProvider.CreateLock(fullKey, options.LockTimeout);

        try
        {
            var retryCount = (int)(options.AcquireTimeout.TotalMilliseconds / Math.Max(1, options.RetryInterval.TotalMilliseconds));
            var retryPolicy = Policy
                .HandleResult<bool>(acquired => !acquired)
                .WaitAndRetryAsync(
                    retryCount: retryCount,
                    sleepDurationProvider: _ => options.RetryInterval);

            var success = await retryPolicy.ExecuteAsync(async (c) => await @lock.AcquireAsync(c).ConfigureAwait(false), ct).ConfigureAwait(false);

            if (success)
            {
                return VKResult.Success(@lock);
            }

            await @lock.DisposeAsync().ConfigureAwait(false);
            return VKResult.Failure<IVKDistributedLock>(VKCachingErrors.LockAcquisitionFailed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "VKError acquiring lock for key: {Key}", fullKey);
            await @lock.DisposeAsync().ConfigureAwait(false);
            return VKResult.Failure<IVKDistributedLock>(VKCachingErrors.LockAcquisitionFailed);
        }
    }
}
