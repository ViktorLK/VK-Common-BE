using VK.Blocks.Blob.Abstractions.Contracts;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Blob.Abstractions;

public interface IBlobFileService
{
    Task<Result<string>> UploadAsync(BlobUploadRequest request, CancellationToken cancellationToken = default);
    Task<Result<BlobDownloadResponse>> DownloadAsync(string blobName, CancellationToken cancellationToken = default);
    Task<Result<BlobDownloadResponse>> DownloadVersionAsync(string blobName, string versionId, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(string blobName, BlobRemoveOptions? deleteOptions = null, CancellationToken cancellationToken = default);
    Task<Result> UndeleteAsync(string blobName, CancellationToken cancellationToken = default);
    Task<Result<bool>> ExistsAsync(string blobName, CancellationToken cancellationToken = default);
    Task<Result<BlobFileMetadata>> GetInfoAsync(string blobName, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<string>>> ListVersionsAsync(string blobName, CancellationToken cancellationToken = default);
    Result<string> GetPublicUrl(string blobName);
}
