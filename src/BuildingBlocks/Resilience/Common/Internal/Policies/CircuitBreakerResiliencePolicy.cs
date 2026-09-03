using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience.Common.Internal.Policies;

// [AP.01] sealed
internal sealed class CircuitBreakerResiliencePolicy : IVKResiliencePolicy
{
    private readonly string _key;
    private readonly TimeSpan? _durationOfBreak;
    private readonly int _minimumThroughput;
    private readonly double _failureRatio;
    private readonly Action<string, TimeSpan, VKResilienceContext>? _onBreak;
    private readonly Action<string, VKResilienceContext>? _onReset;
    private readonly IVKCircuitBreaker _circuitBreaker;

    public VKResilienceMetadata Metadata { get; }

    public CircuitBreakerResiliencePolicy(
        string key,
        TimeSpan? durationOfBreak,
        int minimumThroughput,
        double failureRatio,
        int halfOpenPermittedCalls,
        Action<string, TimeSpan, VKResilienceContext>? onBreak,
        Action<string, VKResilienceContext>? onReset,
        IVKCircuitBreaker circuitBreaker,
        int order = 300)
    {
        _key = VKGuard.NotNullOrWhiteSpace(key);
        _durationOfBreak = durationOfBreak;
        _minimumThroughput = minimumThroughput;
        _failureRatio = failureRatio;
        _onBreak = onBreak;
        _onReset = onReset;
        _circuitBreaker = VKGuard.NotNull(circuitBreaker);

        Metadata = new VKResilienceMetadata
        {
            StrategyName = "CircuitBreaker",
            Key = _key,
            Order = order,
            Description = $"Key: {_key}, MinThroughput: {_minimumThroughput}, FailureRatio: {_failureRatio}"
        };
    }

    public async Task<VKResult<T>> ExecuteAsync<T>(
        Func<VKResilienceContext, CancellationToken, Task<VKResult<T>>> action,
        VKResilienceContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_circuitBreaker.IsAllowed(_key))
        {
            return VKResult.Failure<T>(VKResilienceErrors.CircuitBreakerOpen);
        }

        try
        {
            var result = await action(context, cancellationToken).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                _circuitBreaker.RecordSuccess(_key, _minimumThroughput, k => _onReset?.Invoke(k, context));
            }
            else
            {
                _circuitBreaker.RecordFailure(
                    _key,
                    new InvalidOperationException(result.FirstError.Description),
                    _durationOfBreak,
                    _minimumThroughput,
                    _failureRatio,
                    (k, d) => _onBreak?.Invoke(k, d, context));
            }

            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _circuitBreaker.RecordFailure(
                _key,
                ex,
                _durationOfBreak,
                _minimumThroughput,
                _failureRatio,
                (k, d) => _onBreak?.Invoke(k, d, context));
            throw;
        }
    }

    public async Task<VKResult> ExecuteAsync(
        Func<VKResilienceContext, CancellationToken, Task<VKResult>> action,
        VKResilienceContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_circuitBreaker.IsAllowed(_key))
        {
            return VKResult.Failure(VKResilienceErrors.CircuitBreakerOpen);
        }

        try
        {
            var result = await action(context, cancellationToken).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                _circuitBreaker.RecordSuccess(_key, _minimumThroughput, k => _onReset?.Invoke(k, context));
            }
            else
            {
                _circuitBreaker.RecordFailure(
                    _key,
                    new InvalidOperationException(result.FirstError.Description),
                    _durationOfBreak,
                    _minimumThroughput,
                    _failureRatio,
                    (k, d) => _onBreak?.Invoke(k, d, context));
            }

            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _circuitBreaker.RecordFailure(
                _key,
                ex,
                _durationOfBreak,
                _minimumThroughput,
                _failureRatio,
                (k, d) => _onBreak?.Invoke(k, d, context));
            throw;
        }
    }
}
