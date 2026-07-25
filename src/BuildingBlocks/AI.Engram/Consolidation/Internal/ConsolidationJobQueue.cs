using System.Threading.Channels;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Engram.Consolidation.Internal;

/// <summary>
/// Thread-safe queue for managing consolidation jobs via BoundedChannel.
/// Follows AP.01 (sealed) and AP.03 (internal scoping, no VK prefix).
/// </summary>
internal sealed class ConsolidationJobQueue
{
    private readonly Channel<VKSessionId> _channel;

    public ConsolidationJobQueue()
    {
        var options = new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<VKSessionId>(options);
    }

    /// <summary>
    /// Enqueues a chat session for consolidation. Returns false if the queue is full.
    /// </summary>
    public bool TryEnqueue(VKSessionId sessionId)
    {
        return _channel.Writer.TryWrite(sessionId);
    }

    /// <summary>
    /// Gets the channel reader to consume enqueued session IDs.
    /// </summary>
    public ChannelReader<VKSessionId> Reader => _channel.Reader;
}
