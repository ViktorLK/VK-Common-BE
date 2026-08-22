using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Messaging;

/// <summary>
/// Sender interface for sending commands (1-to-1 queuing).
/// </summary>
public interface IVKCommandSender
{
    /// <summary>
    /// Sends a command to a specific destination queue.
    /// </summary>
    Task<VKResult> SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, IVKCommand;
}
