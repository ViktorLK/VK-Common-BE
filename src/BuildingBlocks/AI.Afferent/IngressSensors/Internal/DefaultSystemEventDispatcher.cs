using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.IngressSensors.Internal;

internal sealed class DefaultSystemEventDispatcher : IVKSystemEventDispatcher
{
    private readonly ConcurrentQueue<VKSystemEvent> _queue = new();
    private readonly VKIngressSensorsOptions _options;

    public DefaultSystemEventDispatcher(IOptionsSnapshot<VKIngressSensorsOptions> options)
    {
        _options = VKGuard.NotNull(options?.Value);
    }

    public Task<VKResult> PublishAsync(VKSystemEvent systemEvent, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(systemEvent);
        cancellationToken.ThrowIfCancellationRequested();

        while (_queue.Count >= _options.MaxEventQueueSize)
        {
            _queue.TryDequeue(out _);
        }

        _queue.Enqueue(systemEvent);
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult<IReadOnlyList<VKSystemEvent>>> ConsumeEventsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var list = new List<VKSystemEvent>();
        while (_queue.TryDequeue(out var ev))
        {
            list.Add(ev);
        }

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKSystemEvent>>(list));
    }
}
