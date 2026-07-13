using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.Cosmos;

/// <summary>
/// Reactor contract for consuming Cosmos Change Feed streams reactively.
/// </summary>
public interface IVKCosmosChangeFeedProcessor
{
    /// <summary>
    /// Starts the change feed processor.
    /// </summary>
    Task<VKResult> StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stops the change feed processor.
    /// </summary>
    Task<VKResult> StopAsync(CancellationToken cancellationToken);
}
