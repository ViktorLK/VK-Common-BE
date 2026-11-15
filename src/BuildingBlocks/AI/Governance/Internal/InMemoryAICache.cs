using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Governance.Internal;

/// <summary>
/// Default in-memory cache implementation for AI governance caching and idempotency.
/// Guarantees safe co-located fallback in the AI contract package (ARCH-05, AP.01, I3).
/// </summary>
internal sealed class InMemoryAICache : IVKAICache
{
    private readonly ConcurrentDictionary<string, string> _cache = new();

    public ValueTask<VKResult<string>> GetAsync(string promptKey, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(promptKey);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromResult(VKResult.Failure<string>(VKAIErrors.Timeout));
        }

        if (_cache.TryGetValue(promptKey, out var value))
        {
            return ValueTask.FromResult(VKResult.Success(value));
        }

        return ValueTask.FromResult(VKResult.Failure<string>(VKAIErrors.CacheNotFound));
    }

    public ValueTask<VKResult<bool>> SetAsync(string promptKey, string response, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(promptKey);
        VKGuard.NotNull(response);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromResult(VKResult.Failure<bool>(VKAIErrors.Timeout));
        }

        _cache[promptKey] = response;
        return ValueTask.FromResult(VKResult.Success(true));
    }
}

