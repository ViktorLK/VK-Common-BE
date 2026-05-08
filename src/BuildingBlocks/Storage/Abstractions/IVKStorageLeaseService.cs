using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Storage;

public interface IVKStorageLeaseService
{
    Task<VKResult<VKStorageLeaseInfo>> AcquireLeaseAsync(string storageName, TimeSpan duration, CancellationToken cancellationToken = default);
    Task<VKResult> ReleaseLeaseAsync(string storageName, string leaseId, CancellationToken cancellationToken = default);
    Task<VKResult<VKStorageLeaseInfo>> RenewLeaseAsync(string storageName, string leaseId, CancellationToken cancellationToken = default);
    Task<VKResult> BreakLeaseAsync(string storageName, CancellationToken cancellationToken = default);
}
