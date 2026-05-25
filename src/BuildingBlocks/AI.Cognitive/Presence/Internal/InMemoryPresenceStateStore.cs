using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Internal in-memory fallback implementation of <see cref="IVKPresenceStateStore"/>.
/// Follows AP.01 (sealed class default) and AP.03 (internal scoping, no VK prefix).
/// </summary>
/// <remarks>
/// [ADR-001 Memory Resumption Implementation]
/// Note: To upgrade to a production-grade distributed state store (e.g., StackExchange.Redis / IDistributedCache):
/// 1. Install Microsoft.Extensions.Caching.StackExchangeRedis package.
/// 2. Inject IDistributedCache and IVKJsonSerializer.
/// 3. In SaveStateAsync:
///    - Serialize the <see cref="VKPresenceState"/> to a JSON string/byte array using IVKJsonSerializer.
///    - Write to Redis via IDistributedCache.SetAsync with a configured TTL (e.g., 2 hours).
/// 4. In LoadStateAsync:
///    - Retrieve the byte array from Redis via IDistributedCache.GetAsync.
///    - Deserialize the byte array back to <see cref="VKPresenceState"/> and return.
/// </remarks>
internal sealed class InMemoryPresenceStateStore : IVKPresenceStateStore
{
    private readonly ConcurrentDictionary<VKPresenceStoreKey, VKPresenceState> _store = new();

    /// <inheritdoc />
    public Task<VKResult> SaveStateAsync(
        VKPresenceStoreKey key,
        VKPresenceState state,
        CancellationToken cancellationToken = default) // [CS.03]
    {
        VKGuard.NotNull(key);
        VKGuard.NotNull(state);

        _store[key] = state;
        return Task.FromResult(VKResult.Success());
    }

    /// <inheritdoc />
    public Task<VKResult<VKPresenceState>> LoadStateAsync(
        VKPresenceStoreKey key,
        CancellationToken cancellationToken = default) // [CS.03]
    {
        VKGuard.NotNull(key);

        if (_store.TryGetValue(key, out var state))
        {
            return Task.FromResult(VKResult.Success(state));
        }

        return Task.FromResult(VKResult.Failure<VKPresenceState>(VKPresenceErrors.SessionNotFound));
    }
}
