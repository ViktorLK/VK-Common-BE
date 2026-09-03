using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience.Common.Internal.Policies;

// [AP.01] sealed
internal sealed class TokenBucketResiliencePolicy : IVKResiliencePolicy
{
    private readonly string _key;
    private readonly double _tokens;
    private readonly TimeSpan? _maxWaitDuration;
    private readonly IVKTokenBucketLimiter _limiter;

    public VKResilienceMetadata Metadata { get; }

    public TokenBucketResiliencePolicy(
        string key,
        double tokens,
        TimeSpan? maxWaitDuration,
        IVKTokenBucketLimiter limiter,
        int order = 450)
    {
        _key = VKGuard.NotNullOrWhiteSpace(key);
        _tokens = tokens;
        _maxWaitDuration = maxWaitDuration;
        _limiter = VKGuard.NotNull(limiter);

        Metadata = new VKResilienceMetadata
        {
            StrategyName = "TokenBucket",
            Key = _key,
            Order = order,
            Description = $"Key: {_key}, Tokens: {_tokens}"
        };
    }

    public async Task<VKResult<T>> ExecuteAsync<T>(
        Func<VKResilienceContext, CancellationToken, Task<VKResult<T>>> action,
        VKResilienceContext context,
        CancellationToken cancellationToken = default)
    {
        var acquireResult = await _limiter.AcquireAsync(_key, _tokens, _maxWaitDuration, cancellationToken).ConfigureAwait(false);
        if (!acquireResult.IsSuccess)
        {
            return VKResult.Failure<T>(acquireResult.FirstError);
        }

        return await action(context, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult> ExecuteAsync(
        Func<VKResilienceContext, CancellationToken, Task<VKResult>> action,
        VKResilienceContext context,
        CancellationToken cancellationToken = default)
    {
        var acquireResult = await _limiter.AcquireAsync(_key, _tokens, _maxWaitDuration, cancellationToken).ConfigureAwait(false);
        if (!acquireResult.IsSuccess)
        {
            return acquireResult;
        }

        return await action(context, cancellationToken).ConfigureAwait(false);
    }
}
