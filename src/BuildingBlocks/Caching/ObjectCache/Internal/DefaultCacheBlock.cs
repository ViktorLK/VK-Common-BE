using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Caching.ObjectCache.Internal;

/// <summary>
/// Production implementation of <see cref="IVKCacheBlock"/> delegating to <see cref="HybridCache"/>.
/// Follows CS.01, CS.03, AP.01, OR.01.
/// </summary>
internal sealed class DefaultCacheBlock(
    HybridCache hybridCache,
    IVKCacheKeyBuilder keyBuilder,
    IVKDistributedLockProvider lockProvider,
    IOptions<VKCachingOptions> cachingOptions,
    ILogger<DefaultCacheBlock> logger) : IVKCacheBlock
{
    private readonly HybridCache _hybridCache = VKGuard.NotNull(hybridCache);
    private readonly IVKCacheKeyBuilder _keyBuilder = VKGuard.NotNull(keyBuilder);
    private readonly IVKDistributedLockProvider _lockProvider = VKGuard.NotNull(lockProvider);
    private readonly VKCachingOptions _options = VKGuard.NotNull(cachingOptions).Value;
    private readonly ILogger<DefaultCacheBlock> _logger = VKGuard.NotNull(logger);

    /// <inheritdoc />
    public async Task<VKResult<VKCacheValue<T>>> GetAsync<T>(string key, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(key);
        using var activity = CachingDiagnostics.StartActivity("Cache.Get");
        var fullKey = _keyBuilder.BuildKey(key);
        activity?.SetTag("cache.key", fullKey);

        try
        {
            var entryOptions = new HybridCacheEntryOptions
            {
                Flags = HybridCacheEntryFlags.DisableUnderlyingData
            };

            var value = await _hybridCache.GetOrCreateAsync<T?>(
                fullKey,
                static _ => ValueTask.FromResult<T?>(default),
                entryOptions,
                cancellationToken: ct
            ).ConfigureAwait(false);

            if (value is null)
            {
                CachingDiagnostics.RecordMiss();
                return VKResult.Success(VKCacheValue<T>.NoValue);
            }

            CachingDiagnostics.RecordHit();
            return VKResult.Success(VKCacheValue<T>.ValueOf(value));
        }
        catch
        {
            CachingDiagnostics.RecordMiss();
            return VKResult.Success(VKCacheValue<T>.NoValue);
        }
    }

    /// <inheritdoc />
    public async Task<VKResult> SetAsync<T>(
        string key,
        T value,
        VKCacheOptions? options = null,
        CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(key);
        using var activity = CachingDiagnostics.StartActivity("Cache.Set");
        var fullKey = _keyBuilder.BuildKey(key);
        activity?.SetTag("cache.key", fullKey);

        try
        {
            HybridCacheEntryOptions? entryOptions = null;
            if (options?.Expiration.HasValue == true || options?.SlidingExpiration.HasValue == true)
            {
                entryOptions = new HybridCacheEntryOptions
                {
                    Expiration = options.Expiration,
                    LocalCacheExpiration = options.Expiration
                };
            }

            var tags = options?.Tags;

            await _hybridCache.SetAsync(
                fullKey,
                value,
                entryOptions,
                tags,
                cancellationToken: ct
            ).ConfigureAwait(false);

            return VKResult.Success();
        }
        catch (Exception ex)
        {
            CachingDiagnostics.RecordError("set");
            _logger.LogCacheOperationFailed(ex, "Set", fullKey);

            if (options?.SuppressErrors == false)
            {
                return VKResult.Failure(VKCachingErrors.ProviderError);
            }

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
        VKGuard.NotNullOrWhiteSpace(key);
        VKGuard.NotNull(factory);
        using var activity = CachingDiagnostics.StartActivity("Cache.GetOrSet");
        var fullKey = _keyBuilder.BuildKey(key);
        activity?.SetTag("cache.key", fullKey);

        try
        {
            HybridCacheEntryOptions? entryOptions = null;
            if (options?.Expiration.HasValue == true || options?.SlidingExpiration.HasValue == true)
            {
                entryOptions = new HybridCacheEntryOptions
                {
                    Expiration = options.Expiration,
                    LocalCacheExpiration = options.Expiration
                };
            }

            var tags = options?.Tags;

            var result = await _hybridCache.GetOrCreateAsync(
                fullKey,
                async cancel => await factory(cancel).ConfigureAwait(false),
                entryOptions,
                tags,
                cancellationToken: ct
            ).ConfigureAwait(false);

            CachingDiagnostics.RecordHit();
            return VKResult.Success(result);
        }
        catch (Exception ex)
        {
            CachingDiagnostics.RecordError("get_or_set");
            _logger.LogCacheOperationFailed(ex, "GetOrSet", fullKey);

            // Manifest Rule 4: Fallback to underlying data source on cache failure
            try
            {
                var direct = await factory(ct).ConfigureAwait(false);
                return VKResult.Success(direct);
            }
            catch
            {
                return VKResult.Failure<T>(VKCachingErrors.ProviderError);
            }
        }
    }

    /// <inheritdoc />
    public async Task<VKResult> RemoveAsync(string key, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(key);
        using var activity = CachingDiagnostics.StartActivity("Cache.Remove");
        var fullKey = _keyBuilder.BuildKey(key);
        activity?.SetTag("cache.key", fullKey);

        try
        {
            await _hybridCache.RemoveAsync(fullKey, ct).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            CachingDiagnostics.RecordError("remove");
            _logger.LogCacheOperationFailed(ex, "Remove", fullKey);
            return VKResult.Failure(VKCachingErrors.ProviderError);
        }
    }

    /// <inheritdoc />
    public async Task<VKResult> RemoveByTagAsync(string tag, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(tag);
        using var activity = CachingDiagnostics.StartActivity("Cache.RemoveByTag");
        activity?.SetTag("cache.tag", tag);

        try
        {
            await _hybridCache.RemoveByTagAsync(tag, ct).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            CachingDiagnostics.RecordError("remove_by_tag");
            _logger.LogCacheOperationFailed(ex, "RemoveByTag", tag);
            return VKResult.Failure(VKCachingErrors.ProviderError);
        }
    }

    /// <inheritdoc />
    public async Task<VKResult<IVKDistributedLockHandle>> AcquireLockAsync(
        string resourceKey,
        VKLockOptions? options = null,
        CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(resourceKey);
        using var activity = CachingDiagnostics.StartActivity("Cache.AcquireLock");
        var fullKey = _keyBuilder.BuildKey($"lock:{resourceKey}");
        activity?.SetTag("cache.lock_key", fullKey);

        try
        {
            var timeout = options?.LockTimeout ?? TimeSpan.FromSeconds(30);
            var handle = await _lockProvider.TryAcquireLockAsync(fullKey, timeout, ct).ConfigureAwait(false);
            if (handle is null)
            {
                return VKResult.Failure<IVKDistributedLockHandle>(VKCachingErrors.LockAcquisitionFailed);
            }

            return VKResult.Success(handle);
        }
        catch (Exception ex)
        {
            CachingDiagnostics.RecordError("lock");
            _logger.LogCacheOperationFailed(ex, "AcquireLock", fullKey);
            return VKResult.Failure<IVKDistributedLockHandle>(VKCachingErrors.LockAcquisitionFailed);
        }
    }
}
