using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience.Common.Internal.Policies;

// [AP.01] sealed
internal sealed class RetryResiliencePolicy : IVKResiliencePolicy
{
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay;
    private readonly double _backoffMultiplier;
    private readonly bool _useJitter;
    private readonly Func<VKError, bool>? _shouldRetry;
    private readonly Action<int, TimeSpan, VKError, VKResilienceContext>? _onRetry;
    private readonly IVKRetryExecutor _executor;

    public VKResilienceMetadata Metadata { get; }

    public RetryResiliencePolicy(
        int maxRetries,
        TimeSpan? initialDelay,
        double backoffMultiplier,
        bool useJitter,
        Func<VKError, bool>? shouldRetry,
        Action<int, TimeSpan, VKError, VKResilienceContext>? onRetry,
        IVKRetryExecutor executor,
        int order = 200)
    {
        _maxRetries = maxRetries;
        _initialDelay = initialDelay ?? TimeSpan.FromMilliseconds(200);
        _backoffMultiplier = backoffMultiplier;
        _useJitter = useJitter;
        _shouldRetry = shouldRetry;
        _onRetry = onRetry;
        _executor = VKGuard.NotNull(executor);

        Metadata = new VKResilienceMetadata
        {
            StrategyName = "Retry",
            Order = order,
            Description = $"MaxRetries: {_maxRetries}, InitialDelay: {_initialDelay.TotalMilliseconds}ms"
        };
    }

    public async Task<VKResult<T>> ExecuteAsync<T>(
        Func<VKResilienceContext, CancellationToken, Task<VKResult<T>>> action,
        VKResilienceContext context,
        CancellationToken cancellationToken = default)
    {
        return await _executor.ExecuteWithRetryAsync(
            ct => action(context, ct),
            maxRetries: _maxRetries,
            initialDelay: _initialDelay,
            shouldRetry: _shouldRetry,
            onRetry: (attempt, delay, error) => _onRetry?.Invoke(attempt, delay, error, context),
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult> ExecuteAsync(
        Func<VKResilienceContext, CancellationToken, Task<VKResult>> action,
        VKResilienceContext context,
        CancellationToken cancellationToken = default)
    {
        return await _executor.ExecuteWithRetryAsync(
            ct => action(context, ct),
            maxRetries: _maxRetries,
            initialDelay: _initialDelay,
            shouldRetry: _shouldRetry,
            onRetry: (attempt, delay, error) => _onRetry?.Invoke(attempt, delay, error, context),
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
