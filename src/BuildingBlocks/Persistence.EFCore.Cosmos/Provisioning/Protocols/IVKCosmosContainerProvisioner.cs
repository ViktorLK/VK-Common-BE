using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.Cosmos;

/// <summary>
/// Public interface for dynamically provisioning Cosmos DB containers.
/// </summary>
public interface IVKCosmosContainerProvisioner
{
    /// <summary>
    /// Provisions a container based on the provided definition.
    /// </summary>
    Task<VKResult> ProvisionContainerAsync(
        VKCosmosContainerDefinition definition,
        CancellationToken ct);
}
