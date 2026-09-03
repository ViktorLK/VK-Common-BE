using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience.Common.Internal.Policies;

// [AP.01] sealed
internal sealed class TimeoutResiliencePolicy : IVKResiliencePolicy
{
    private readonly TimeSpan _timeout;
    private readonly bool _isPessimistic;
    private readonly Action<TimeSpan, VKResilienceContext>? _onTimeout;
    private readonly IVKTimeoutExecutor _executor;

    public VKResilienceMetadata Metadata { get; }

    public TimeoutResiliencePolicy(
        TimeSpan timeout,
        bool isPessimistic,
        Action<TimeSpan, VKResilienceContext>? onTimeout,
        IVKTimeoutExecutor executor,
        int order = 100)
    {
        _timeout = timeout;
        _isPessimistic = isPessimistic;
        _onTimeout = onTimeout;
        _executor = VKGuard.NotNull(executor);

        Metadata = new VKResilienceMetadata
        {
            StrategyName = "Timeout",
            Order = order,
            Description = $"Timeout: {_timeout.TotalSeconds}s (Pessimistic: {_isPessimistic})"
        };
    }

    public async Task<VKResult<T>> ExecuteAsync<T>(
        Func<VKResilienceContext, CancellationToken, Task<VKResult<T>>> action,
        VKResilienceContext context,
        CancellationToken cancellationToken = default)
    {
        return await _executor.ExecuteWithTimeoutAsync(
            ct => action(context, ct),
            timeout: _timeout,
            isPessimistic: _isPessimistic,
            onTimeout: d => _onTimeout?.Invoke(d, context),
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult> ExecuteAsync(
        Func<VKResilienceContext, CancellationToken, Task<VKResult>> action,
        VKResilienceContext context,
        CancellationToken cancellationToken = default)
    {
        return await _executor.ExecuteWithTimeoutAsync(
            ct => action(context, ct),
            timeout: _timeout,
            isPessimistic: _isPessimistic,
            onTimeout: d => _onTimeout?.Invoke(d, context),
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
