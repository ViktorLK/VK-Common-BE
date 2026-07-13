using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Persistence.Cosmos;

/// <summary>
/// User-implementable handler for processing Cosmos DB Change Feed events.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IVKChangeFeedHandler<T> where T : class
{
    /// <summary>
    /// Processes a collection of changed documents.
    /// </summary>
    /// <param name="changes">The read-only collection of changes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleChangesAsync(
        IReadOnlyCollection<T> changes,
        CancellationToken cancellationToken);
}
