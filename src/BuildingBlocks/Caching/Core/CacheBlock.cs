using VK.Blocks.Caching.Abstractions;
using VK.Blocks.Caching.Abstractions.Contracts;
using VK.Blocks.Caching.Core;
using VK.Blocks.Caching.Diagnostics;
using VK.Blocks.Caching.Options;
using VK.Blocks.Caching.Resilience.Avalanche;
using VK.Blocks.Caching.Resilience.Breakdown;
using VK.Blocks.Caching.Resilience.Penetration;
using VK.Blocks.Caching.Serialization;
using VK.Blocks.Core.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Polly;

namespace VK.Blocks.Caching.Core;

/// <summary>
/// Core implementation of ICacheBlock that orchestrates providers, serialization, and resilience.
/// </summary>
public sealed class CacheBlock(
    ICacheProvider provider,
    ICacheSerializer serializer,
    IDistributedLockProvider lockProvider,
    ICacheKeyBuilder keyBuilder,
    IJitterExpiryStrategy jitterStrategy,
    ILockBasedGuard breakdownGuard,
    INullValueGuard penetrationGuard,
    IOptions<CachingOptions> cachingOptions,
    IOptions<ResilienceOptions> resilienceOptions,
    ILogger<CacheBlock> logger) : ICacheBlock
{
    private readonly CachingOptions _cachingOptions = cachingOptions.Value;
    private readonly ResilienceOptions _resilienceOptions = resilienceOptions.Value;

    public async Task<Result<CacheValue<T>>> GetAsync<T>(string key, CancellationToken ct = default)
    {
        using var activity = CachingDiagnostics.StartActivity("Cache.Get");
        var fullKey = keyBuilder.BuildKey(key);
        activity?.SetTag("cache.key", fullKey);

        try
        {
            var providerResult = await provider.GetAsync(fullKey, ct);
            if (providerResult.IsFailure)
            {
                CachingDiagnostics.CacheErrors.Add(1, new TagList { { "operation", "get" } });
                return Result.Failure<CacheValue<T>>(providerResult.Errors);
            }

            var bytes = providerResult.Value;
            if (bytes == null)
            {
                CachingDiagnostics.CacheMisses.Add(1);
                return Result.Success(CacheValue<T>.NoValue);
            }

            var entry = serializer.Deserialize<CacheEntry<T>>(bytes);
            if (entry == null)
            {
                CachingDiagnostics.CacheMisses.Add(1);
                return Result.Success(CacheValue<T>.NoValue);
            }

            if (entry.IsExpired(DateTimeOffset.UtcNow))
            {
                await provider.RemoveAsync(fullKey, ct);
                CachingDiagnostics.CacheMisses.Add(1);
                return Result.Success(CacheValue<T>.NoValue);
            }

            CachingDiagnostics.CacheHits.Add(1);
            return Result.Success(CacheValue<T>.ValueOf(penetrationGuard.Unwrap<T>(entry.Value)));
        }
        catch (Exception ex)
        {
            CachingDiagnostics.CacheErrors.Add(1, new TagList { { "operation", "get" } });
            logger.LogError(ex, "Cache get error for key: {Key}", fullKey);
            return Result.Failure<CacheValue<T>>(CachingErrors.ProviderError);
        }
    }

    public async Task<Result> SetAsync<T>(string key, T value, CacheOptions? options = null, CancellationToken ct = default)
    {
        using var activity = CachingDiagnostics.StartActivity("Cache.Set");
        var fullKey = keyBuilder.BuildKey(key);
        options ??= CacheOptions.Default;
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
                DateTimeOffset.UtcNow.Add(expiration),
                options.SlidingExpiration);

            var bytes = serializer.Serialize(entry);
            var providerResult = await provider.SetAsync(fullKey, bytes, options, ct);
            
            return providerResult;
        }
        catch (Exception ex)
        {
            CachingDiagnostics.CacheErrors.Add(1, new TagList { { "operation", "set" } });
            logger.LogError(ex, "Cache set error for key: {Key}", fullKey);
            
            if (!options.SuppressErrors)
                return Result.Failure(CachingErrors.ProviderError);
                
            return Result.Success();
        }
    }

    public async Task<Result<T>> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CacheOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= CacheOptions.Default;
        var fullKey = keyBuilder.BuildKey(key);

        if (!options.BypassCache)
        {
            var cachedResult = await GetAsync<T>(key, ct);
            if (cachedResult.IsSuccess && cachedResult.Value!.HasValue)
            {
                var value = cachedResult.Value.Value;
                if (value != null) return Result.Success(value);
            }
        }

        if (_resilienceOptions.EnableBreakdownProtection)
        {
            var breakdownResult = await breakdownGuard.ExecuteAsync<T>(
                lockProvider,
                key,
                async (c) => 
                {
                    var res = await factory(c);
                    await SetAsync(key, res, options, c);
                    return Result.Success(res);
                },
                () => GetAsync<T>(key, ct),
                ct);
            
            if (breakdownResult.IsSuccess && breakdownResult.Value!.HasValue)
            {
                var value = breakdownResult.Value.Value;
                if (value != null) return Result.Success(value);
            }
        }

        var result = await factory(ct);
        await SetAsync(key, result, options, ct);
        return Result.Success(result);
    }

    public async Task<Result> RemoveAsync(string key, CancellationToken ct = default)
    {
        using var activity = CachingDiagnostics.StartActivity("Cache.Remove");
        var fullKey = keyBuilder.BuildKey(key);
        activity?.SetTag("cache.key", fullKey);
        
        return await provider.RemoveAsync(fullKey, ct);
    }

    public async Task<Result<IDistributedLock>> AcquireLockAsync(string resourceKey, LockOptions? options = null, CancellationToken ct = default)
    {
        using var activity = CachingDiagnostics.StartActivity("Cache.AcquireLock");
        options ??= new LockOptions();
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

            var success = await retryPolicy.ExecuteAsync(async (c) => await @lock.AcquireAsync(c), ct);

            if (success)
            {
                return Result.Success(@lock);
            }

            await @lock.DisposeAsync();
            return Result.Failure<IDistributedLock>(CachingErrors.LockAcquisitionFailed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error acquiring lock for key: {Key}", fullKey);
            await @lock.DisposeAsync();
            return Result.Failure<IDistributedLock>(CachingErrors.LockAcquisitionFailed);
        }
    }
}
