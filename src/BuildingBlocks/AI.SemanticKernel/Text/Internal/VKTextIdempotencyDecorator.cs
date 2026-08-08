using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Text.Internal;

/// <summary>
/// Decorator for <see cref="IVKTextEngine"/> to support request idempotency control.
/// </summary>
internal sealed class VKTextIdempotencyDecorator : IVKTextEngine
{
    private readonly IVKTextEngine _inner;
    private readonly IVKAICache _cache;

    public VKTextIdempotencyDecorator(IVKTextEngine inner, IVKAICache cache)
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
        string? idempotencyKey = GetIdempotencyKey(args);
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return _inner.GenerateAsync(prompt, args, cancellationToken);
        }

        return ExecuteIdempotentAsync(idempotencyKey, () => _inner.GenerateAsync(prompt, args, cancellationToken), cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VKResult<VKTextResponse>> GenerateStreamingAsync(
        string prompt,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        return _inner.GenerateStreamingAsync(prompt, args, cancellationToken);
    }

    private async Task<VKResult<VKTextResponse>> ExecuteIdempotentAsync(
        string key,
        Func<Task<VKResult<VKTextResponse>>> invokeUnderlying,
        CancellationToken cancellationToken)
    {
        string cacheKey = $"Idempotency:Text:{key}";

        // 1. Try get cached completed result
        VKResult<string> cacheVal = await _cache.GetAsync(cacheKey, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (cacheVal.IsSuccess && !string.IsNullOrWhiteSpace(cacheVal.Value))
        {
            if (cacheVal.Value == "IN_PROGRESS")
            {
                return VKResult.Failure<VKTextResponse>(new VKError(
                    "AI.Text.RequestInFlight",
                    "An identical request is already in progress. Please try again."));
            }

            try
            {
                VKTextResponse? cachedResponse = JsonSerializer.Deserialize<VKTextResponse>(cacheVal.Value);
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
            VKResult<VKTextResponse> result = await invokeUnderlying().ConfigureAwait(false);
            if (result.IsSuccess)
            {
                string serialized = JsonSerializer.Serialize(result.Value);
                await _cache.SetAsync(cacheKey, serialized, cancellationToken).ConfigureAwait(false);
            }
            else
            {
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
