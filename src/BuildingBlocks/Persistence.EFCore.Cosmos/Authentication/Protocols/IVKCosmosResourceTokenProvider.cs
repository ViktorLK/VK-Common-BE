using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Cosmos;

/// <summary>
/// Public interface for dynamically fetching resource tokens.
/// </summary>
public interface IVKCosmosResourceTokenProvider
{
    /// <summary>
    /// Gets the resource token for the specified user and container.
    /// </summary>
    Task<VKResult<string>> GetResourceTokenAsync(
        string userId,
        string containerName,
        CancellationToken ct);
}
