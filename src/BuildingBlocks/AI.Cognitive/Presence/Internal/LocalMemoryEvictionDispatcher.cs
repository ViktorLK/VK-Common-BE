using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Default in-memory dispatcher utilizing System.Threading.Channels for safe closed-loop thread handoff.
/// Follows CS.03 (async, ValueTask hot-paths), AP.01 (sealed class, VKGuard), and AP.03.
/// </summary>
internal sealed class LocalMemoryEvictionDispatcher : IVKMemoryEvictionDispatcher
{
    private readonly Channel<VKMemoryEvictionEvent> _channel;

    /// <summary>
    /// Gets the channel reader to consume evicted turns out-of-band.
    /// </summary>
    public ChannelReader<VKMemoryEvictionEvent> Reader => _channel.Reader;

    public LocalMemoryEvictionDispatcher()
    {
        // Thread-safe unbounded channel optimized for single-reader out-of-band processing
        _channel = Channel.CreateUnbounded<VKMemoryEvictionEvent>(new UnboundedChannelOptions
        {
            SingleWriter = false,
            SingleReader = true
        });
    }

    /// <inheritdoc />
    public async ValueTask DispatchAsync(VKMemoryEvictionEvent @event, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(@event); // [AP.01] Boundary check

        await _channel.Writer.WriteAsync(@event, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
    }
}
