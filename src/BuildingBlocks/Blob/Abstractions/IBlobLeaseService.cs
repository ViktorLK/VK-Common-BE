using VK.Blocks.Blob.Abstractions.Contracts;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Blob.Abstractions;

public interface IBlobLeaseService
{
    Task<Result<BlobLeaseInfo>> AcquireLeaseAsync(string blobName, TimeSpan duration, CancellationToken cancellationToken = default);
    Task<Result> ReleaseLeaseAsync(string blobName, string leaseId, CancellationToken cancellationToken = default);
    Task<Result<BlobLeaseInfo>> RenewLeaseAsync(string blobName, string leaseId, CancellationToken cancellationToken = default);
    Task<Result> BreakLeaseAsync(string blobName, CancellationToken cancellationToken = default);
}
