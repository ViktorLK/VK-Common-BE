using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience.Common.Internal.Policies;

// [AP.01] sealed
internal sealed class RateLimiterResiliencePolicy : IVKResiliencePolicy
{
    private readonly string _key;
    private readonly int _permitLimit;
    private readonly TimeSpan? _window;
    private readonly IVKRateLimiter _limiter;

    public VKResilienceMetadata Metadata { get; }

    public RateLimiterResiliencePolicy(
        string key,
        int permitLimit,
        TimeSpan? window,
        IVKRateLimiter limiter,
        int order = 400)
    {
        _key = VKGuard.NotNullOrWhiteSpace(key);
        _permitLimit = permitLimit;
        _window = window;
        _limiter = VKGuard.NotNull(limiter);

        Metadata = new VKResilienceMetadata
        {
            StrategyName = "RateLimiter",
            Key = _key,
            Order = order,
            Description = $"Key: {_key}, Limit: {_permitLimit}, Window: {_window?.TotalSeconds ?? 60}s"
        };
    }

    public async Task<VKResult<T>> ExecuteAsync<T>(
        Func<VKResilienceContext, CancellationToken, Task<VKResult<T>>> action,
        VKResilienceContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_limiter.IsAllowed(_key, _permitLimit, _window))
        {
            return VKResult.Failure<T>(VKResilienceErrors.RateLimitExceeded);
        }

        _limiter.RecordRequest(_key);
        return await action(context, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult> ExecuteAsync(
        Func<VKResilienceContext, CancellationToken, Task<VKResult>> action,
        VKResilienceContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_limiter.IsAllowed(_key, _permitLimit, _window))
        {
            return VKResult.Failure(VKResilienceErrors.RateLimitExceeded);
        }

        _limiter.RecordRequest(_key);
        return await action(context, cancellationToken).ConfigureAwait(false);
    }
}
