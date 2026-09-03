using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience.Common.Internal.Policies;

// [AP.01] sealed
internal sealed class BulkheadResiliencePolicy : IVKResiliencePolicy
{
    private readonly string _key;
    private readonly int _maxParallelization;
    private readonly int _maxQueuedCount;
    private readonly TimeSpan? _queueTimeout;
    private readonly IVKBulkhead _bulkhead;

    public VKResilienceMetadata Metadata { get; }

    public BulkheadResiliencePolicy(
        string key,
        int maxParallelization,
        int maxQueuedCount,
        TimeSpan? queueTimeout,
        IVKBulkhead bulkhead,
        int order = 500)
    {
        _key = VKGuard.NotNullOrWhiteSpace(key);
        _maxParallelization = maxParallelization;
        _maxQueuedCount = maxQueuedCount;
        _queueTimeout = queueTimeout;
        _bulkhead = VKGuard.NotNull(bulkhead);

        Metadata = new VKResilienceMetadata
        {
            StrategyName = "Bulkhead",
            Key = _key,
            Order = order,
            Description = $"Key: {_key}, MaxParallelism: {_maxParallelization}, MaxQueued: {_maxQueuedCount}"
        };
    }

    public async Task<VKResult<T>> ExecuteAsync<T>(
        Func<VKResilienceContext, CancellationToken, Task<VKResult<T>>> action,
        VKResilienceContext context,
        CancellationToken cancellationToken = default)
    {
        var acquireResult = await _bulkhead.AcquireAsync(
            _key,
            _maxParallelization,
            _maxQueuedCount,
            _queueTimeout,
            cancellationToken).ConfigureAwait(false);

        if (!acquireResult.IsSuccess)
        {
            return VKResult.Failure<T>(acquireResult.FirstError);
        }

        try
        {
            return await action(context, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _bulkhead.Release(_key);
        }
    }

    public async Task<VKResult> ExecuteAsync(
        Func<VKResilienceContext, CancellationToken, Task<VKResult>> action,
        VKResilienceContext context,
        CancellationToken cancellationToken = default)
    {
        var acquireResult = await _bulkhead.AcquireAsync(
            _key,
            _maxParallelization,
            _maxQueuedCount,
            _queueTimeout,
            cancellationToken).ConfigureAwait(false);

        if (!acquireResult.IsSuccess)
        {
            return acquireResult;
        }

        try
        {
            return await action(context, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _bulkhead.Release(_key);
        }
    }
}
