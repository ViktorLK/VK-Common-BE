using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Chat.Internal;

/// <summary>
/// Decorator for <see cref="IVKChatEngine"/> to support prompt caching.
/// </summary>
internal sealed class VKChatCachingDecorator : IVKChatEngine
{
    private readonly IVKChatEngine _inner;
    private readonly IVKAICache _cache;

    public VKChatCachingDecorator(IVKChatEngine inner, IVKAICache cache)
    {
        _inner = VKGuard.NotNull(inner);
        _cache = VKGuard.NotNull(cache);
    }

    /// <inheritdoc />
    public Task<VKResult<VKChatResponse>> SendAsync(
        IEnumerable<VKChatMessage> messages,
        VKChatArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        string cacheKey = GenerateCacheKey(messages);
        return SendCachedAsync(cacheKey, () => _inner.SendAsync(messages, args, cancellationToken), cancellationToken);
    }

    /// <inheritdoc />
    public Task<VKResult<VKChatResponse>> SendAsync(
        VKContextPayload payload,
        VKChatArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(payload);
        if (!payload.EnableContextCaching)
        {
            return _inner.SendAsync(payload, args, cancellationToken);
        }

        return SendCachedAsync(payload.ContextCacheKey, () => _inner.SendAsync(payload, args, cancellationToken), cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        IEnumerable<VKChatMessage> messages,
        VKChatArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        return _inner.SendStreamingAsync(messages, args, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        VKContextPayload payload,
        VKChatArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        return _inner.SendStreamingAsync(payload, args, cancellationToken);
    }

    /// <inheritdoc />
    public Task<VKResult<VKStructuredChatResponse<T>>> SendStructuredAsync<T>(
        IEnumerable<VKChatMessage> messages,
        VKChatArgs? args = null,
        CancellationToken cancellationToken = default) where T : class
    {
        return _inner.SendStructuredAsync<T>(messages, args, cancellationToken);
    }

    private async Task<VKResult<VKChatResponse>> SendCachedAsync(
        string key,
        Func<Task<VKResult<VKChatResponse>>> invokeUnderlying,
        CancellationToken cancellationToken)
    {
        VKResult<string> cacheResult = await _cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
        if (cacheResult.IsSuccess && !string.IsNullOrWhiteSpace(cacheResult.Value))
        {
            try
            {
                VKChatResponse? cachedResponse = JsonSerializer.Deserialize<VKChatResponse>(cacheResult.Value);
                if (cachedResponse is not null)
                {
                    return VKResult.Success(cachedResponse);
                }
            }
            catch
            {
                // Fallback
            }
        }

        VKResult<VKChatResponse> result = await invokeUnderlying().ConfigureAwait(false);
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

    private static string GenerateCacheKey(IEnumerable<VKChatMessage> messages)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        Span<char> initialBuffer = stackalloc char[512];
        using var sb = new VKValueStringBuilder(initialBuffer);
        foreach (VKChatMessage msg in messages)
        {
            sb.Append(msg.Role.ToString());
            sb.Append(':');
            sb.Append(msg.Content);
            sb.Append('|');
        }
        byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(bytes);
    }
}
