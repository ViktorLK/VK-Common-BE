using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Text.Internal;

/// <summary>
/// Decorator for <see cref="IVKTextEngine"/> to support prompt caching.
/// </summary>
internal sealed class VKTextCachingDecorator : IVKTextEngine
{
    private readonly IVKTextEngine _inner;
    private readonly IVKAICache _cache;

    public VKTextCachingDecorator(IVKTextEngine inner, IVKAICache cache)
    {
        _inner = VKGuard.NotNull(inner);
        _cache = VKGuard.NotNull(cache);
    }

    /// <inheritdoc />
    public Task<VKResult<VKTextResponse>> GenerateAsync(
        string prompt,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(prompt);
        string cacheKey = GenerateCacheKey(prompt);
        return SendCachedAsync(cacheKey, () => _inner.GenerateAsync(prompt, args, cancellationToken), cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VKResult<VKTextResponse>> GenerateStreamingAsync(
        string prompt,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        return _inner.GenerateStreamingAsync(prompt, args, cancellationToken);
    }

    private async Task<VKResult<VKTextResponse>> SendCachedAsync(
        string key,
        Func<Task<VKResult<VKTextResponse>>> invokeUnderlying,
        CancellationToken cancellationToken)
    {
        VKResult<string> cacheResult = await _cache.GetAsync(key, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (cacheResult.IsSuccess && !string.IsNullOrWhiteSpace(cacheResult.Value))
        {
            try
            {
                VKTextResponse? cachedResponse = JsonSerializer.Deserialize<VKTextResponse>(cacheResult.Value);
                if (cachedResponse is not null)
                {
                    return VKResult.Success(cachedResponse);
                }
            }
            catch
            {
                // Fallback to underlying on deserialization failure
            }
        }

        VKResult<VKTextResponse> result = await invokeUnderlying().ConfigureAwait(false);
        if (result.IsSuccess)
        {
            try
            {
                string serialized = JsonSerializer.Serialize(result.Value);
                await _cache.SetAsync(key, serialized, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Best effort
            }
        }

        return result;
    }

    private static string GenerateCacheKey(string prompt)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(prompt));
        return Convert.ToHexString(bytes);
    }
}
