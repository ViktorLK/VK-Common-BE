using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Cosmos;

/// <summary>
/// Public interface for managing or monitoring Cosmos DB failover regions.
/// </summary>
public interface IVKCosmosFailoverManager
{
    /// <summary>
    /// Gets the list of available read regions configured on the Cosmos DB account.
    /// </summary>
    Task<VKResult<IReadOnlyList<string>>> GetAvailableRegionsAsync(CancellationToken ct);
}
