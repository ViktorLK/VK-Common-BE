using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Workflow.Recovery.Internal;

/// <summary>
/// Internal bounded Channel queue for passing orphan workflow items from scan loop to process loop.
/// Follows AP.01.
/// </summary>
internal sealed class WorkflowOrphanScanJobQueue
{
    private readonly Channel<VKWorkflowInstance> _channel;

    public WorkflowOrphanScanJobQueue()
    {
        var options = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true
        };
        _channel = Channel.CreateBounded<VKWorkflowInstance>(options);
    }

    public ValueTask EnqueueAsync(VKWorkflowInstance instance, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(instance);
        return _channel.Writer.WriteAsync(instance, cancellationToken);
    }

    public ValueTask<VKWorkflowInstance> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}
