using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Storage;

public interface IVKStorageDirectoryService
{
    Task<VKResult> CreateDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    Task<VKResult> DeleteDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    Task<VKResult<bool>> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default);

    Task<VKResult<IReadOnlyList<VKStorageEntry>>> ListHierarchyAsync(
        string? prefix = null,
        string delimiter = "/",
        CancellationToken cancellationToken = default);
}
