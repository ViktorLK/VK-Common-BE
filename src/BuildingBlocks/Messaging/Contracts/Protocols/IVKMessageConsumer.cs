using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Messaging;

/// <summary>
/// Defines the contract for starting and stopping a message consumer background engine.
/// </summary>
public interface IVKMessageConsumer
{
    /// <summary>
    /// Starts the consumption.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the consumption.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}
