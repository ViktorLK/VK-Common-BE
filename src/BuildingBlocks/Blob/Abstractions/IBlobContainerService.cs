using VK.Blocks.Core.Results;

namespace VK.Blocks.Blob.Abstractions;

public interface IBlobContainerService
{
    Task<Result> CreateContainerAsync(string containerName, CancellationToken cancellationToken = default);
    Task<Result> DeleteContainerAsync(string containerName, CancellationToken cancellationToken = default);
    Task<Result<bool>> ContainerExistsAsync(string containerName, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<string>>> ListContainersAsync(string? prefix = null, CancellationToken cancellationToken = default);
}
