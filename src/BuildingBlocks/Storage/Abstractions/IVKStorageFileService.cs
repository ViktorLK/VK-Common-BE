using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Storage;

public interface IVKStorageFileService
{
    Task<VKResult<string>> UploadAsync(VKStorageUploadRequest request, CancellationToken cancellationToken = default);
    Task<VKResult<VKStorageDownloadResponse>> DownloadAsync(string storageName, CancellationToken cancellationToken = default);
    Task<VKResult<VKStorageDownloadResponse>> DownloadVersionAsync(string storageName, string versionId, CancellationToken cancellationToken = default);
    Task<VKResult> DeleteAsync(string storageName, VKStorageRemoveOptions? deleteOptions = null, CancellationToken cancellationToken = default);
    Task<VKResult> UndeleteAsync(string storageName, CancellationToken cancellationToken = default);
    Task<VKResult<bool>> ExistsAsync(string storageName, CancellationToken cancellationToken = default);
    Task<VKResult<VKStorageFileMetadata>> GetInfoAsync(string storageName, CancellationToken cancellationToken = default);
    Task<VKResult<IReadOnlyList<string>>> ListVersionsAsync(string storageName, CancellationToken cancellationToken = default);
    VKResult<string> GetPublicUrl(string storageName);
}
