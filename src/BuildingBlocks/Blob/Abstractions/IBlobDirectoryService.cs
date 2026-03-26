using VK.Blocks.Blob.Abstractions.Contracts;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Blob.Abstractions;

public interface IBlobDirectoryService
{
    Task<Result> CreateDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);
    
    Task<Result> DeleteDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    Task<Result<bool>> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<BlobEntry>>> ListHierarchyAsync(
        string? prefix = null, 
        string delimiter = "/", 
        CancellationToken cancellationToken = default);
}
