using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.Cosmos.Connection;

/// <summary>
/// Exposes the bottom physical DB connection.
/// </summary>
public interface IVKCosmosDbConnection
{
    /// <summary>
    /// Gets the Cosmos client.
    /// </summary>
    CosmosClient Client { get; }

    /// <summary>
    /// Gets the Cosmos database.
    /// </summary>
    Database Database { get; }

    /// <summary>
    /// Gets a container reference.
    /// </summary>
    Container GetContainer(string containerName);

    /// <summary>
    /// Initializes database and standard structures.
    /// </summary>
    Task<VKResult> InitializeAsync(CancellationToken cancellationToken);
}
