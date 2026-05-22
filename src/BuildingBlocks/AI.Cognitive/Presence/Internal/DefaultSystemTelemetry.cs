using System;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Default fallback implementation of <see cref="IVKSystemTelemetry"/> that reports provider status as always healthy.
/// Follows AP.01, AP.03, and CS.03.
/// </summary>
internal sealed class DefaultSystemTelemetry : IVKSystemTelemetry
{
    /// <inheritdoc />
    public Task RecordLatencyAsync(
        string provider,
        TimeSpan latency,
        bool isSuccess,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> IsProviderStressedAsync(
        string provider,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(false);
    }
}
