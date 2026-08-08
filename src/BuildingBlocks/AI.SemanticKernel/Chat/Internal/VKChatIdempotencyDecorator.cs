using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Chat.Internal;

/// <summary>
/// Decorator for <see cref="IVKChatEngine"/> to support request idempotency control.
/// </summary>
internal sealed class VKChatIdempotencyDecorator : IVKChatEngine
{
    private readonly IVKChatEngine _inner;
    private readonly IVKAICache _cache;

    public VKChatIdempotencyDecorator(IVKChatEngine inner, IVKAICache cache)
    {
        _inner = VKGuard.NotNull(inner);
        _cache = VKGuard.NotNull(cache);
    }

    /// <inheritdoc />
    public Task<VKResult<VKChatResponse>> SendAsync(
        IEnumerable<VKChatMessage> messages,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        string? idempotencyKey = GetIdempotencyKey(args);
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return _inner.SendAsync(messages, args, cancellationToken);
        }

        return ExecuteIdempotentAsync(idempotencyKey, () => _inner.SendAsync(messages, args, cancellationToken), cancellationToken);
    }

    /// <inheritdoc />
    public Task<VKResult<VKChatResponse>> SendAsync(
        VKContextPayload payload,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        string? idempotencyKey = GetIdempotencyKey(args);
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return _inner.SendAsync(payload, args, cancellationToken);
        }

        return ExecuteIdempotentAsync(idempotencyKey, () => _inner.SendAsync(payload, args, cancellationToken), cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        IEnumerable<VKChatMessage> messages,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        return _inner.SendStreamingAsync(messages, args, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VKResult<VKChatStreamingResponse>> SendStreamingAsync(
        VKContextPayload payload,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        return _inner.SendStreamingAsync(payload, args, cancellationToken);
    }

    /// <inheritdoc />
    public Task<VKResult<VKStructuredChatResponse<T>>> SendStructuredAsync<T>(
        IEnumerable<VKChatMessage> messages,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default) where T : class
    {
        return _inner.SendStructuredAsync<T>(messages, args, cancellationToken);
    }

    private async Task<VKResult<VKChatResponse>> ExecuteIdempotentAsync(
        string key,
        Func<Task<VKResult<VKChatResponse>>> invokeUnderlying,
        CancellationToken cancellationToken)
    {
        string cacheKey = $"Idempotency:Chat:{key}";

        // 1. Try get cached completed result
        VKResult<string> cacheVal = await _cache.GetAsync(cacheKey, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (cacheVal.IsSuccess && !string.IsNullOrWhiteSpace(cacheVal.Value))
        {
            if (cacheVal.Value == "IN_PROGRESS")
            {
                return VKResult.Failure<VKChatResponse>(new VKError(
                    "AI.Chat.RequestInFlight",
                    "An identical request is already in progress. Please try again."));
            }

            try
            {
                VKChatResponse? cachedResponse = JsonSerializer.Deserialize<VKChatResponse>(cacheVal.Value);
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

        // 2. Mark in-progress
        await _cache.SetAsync(cacheKey, "IN_PROGRESS", cancellationToken).ConfigureAwait(false);

        try
        {
            VKResult<VKChatResponse> result = await invokeUnderlying().ConfigureAwait(false);
            if (result.IsSuccess)
            {
                string serialized = JsonSerializer.Serialize(result.Value);
                await _cache.SetAsync(cacheKey, serialized, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Remove in-progress marker on failure to allow retry
                await _cache.SetAsync(cacheKey, string.Empty, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }
        catch
        {
            await _cache.SetAsync(cacheKey, string.Empty, cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    private static string? GetIdempotencyKey(IVKAIArgs? args)
    {
        if (args?.Context is not null && args.Context.TryGetValue("IdempotencyKey", out var val) && val is not null)
        {
            return val.ToString();
        }
        return null;
    }
}
