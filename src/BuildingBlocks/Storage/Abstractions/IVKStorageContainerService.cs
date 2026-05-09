using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Storage;

public interface IVKStorageContainerService
{
    Task<VKResult> CreateContainerAsync(string containerName, CancellationToken cancellationToken = default);
    Task<VKResult> DeleteContainerAsync(string containerName, CancellationToken cancellationToken = default);
    Task<VKResult<bool>> ContainerExistsAsync(string containerName, CancellationToken cancellationToken = default);
    Task<VKResult<IReadOnlyList<string>>> ListContainersAsync(string? prefix = null, CancellationToken cancellationToken = default);
}
