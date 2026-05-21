using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// A high-performance, thread-safe in-memory sliding window rate limiter.
/// Restricts user and tenant requests to a configurable threshold per second.
/// Follows AP.01, AP.03, CS.03, and CS.06.
/// </summary>
internal sealed class InMemoryRateLimiter : IVKPresenceRateLimiter
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<long>> _requests = [];
    private readonly TimeProvider _timeProvider;
    private const int MaxRequestsPerSecond = 5;

    public InMemoryRateLimiter(TimeProvider timeProvider)
    {
        _timeProvider = VKGuard.NotNull(timeProvider);
    }

    /// <inheritdoc />
    public Task<VKResult> AuditRateLimitAsync(
        string tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(tenantId);
        VKGuard.NotNullOrWhiteSpace(userId);

        var key = $"{tenantId}:{userId}";
        var nowTicks = _timeProvider.GetUtcNow().Ticks;
        var oneSecondAgoTicks = nowTicks - TimeSpan.TicksPerSecond;

        var queue = _requests.GetOrAdd(key, _ => new ConcurrentQueue<long>());

        lock (queue)
        {
            // Remove timestamps older than 1 second
            while (queue.TryPeek(out var timestamp) && timestamp < oneSecondAgoTicks)
            {
                queue.TryDequeue(out _);
            }

            if (queue.Count >= MaxRequestsPerSecond)
            {
                return Task.FromResult(VKResult.Failure(PresenceErrors.TooManyRequests)); // [CS.01]
            }

            queue.Enqueue(nowTicks);
        }

        return Task.FromResult(VKResult.Success());
    }
}
