using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Storage.Azure.Diagnostics.Internal;

namespace VK.Blocks.Storage.Azure.Internal.Services;

internal sealed class StorageLeaseService(
    IVKStorageContainerProvider containerProvider,
    BlobServiceClient BlobServiceClient,
    ILogger<StorageLeaseService> logger) : IVKStorageLeaseService
{
    public async Task<VKResult<VKStorageLeaseInfo>> AcquireLeaseAsync(string storageName, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(AcquireLeaseAsync));
        activity?.SetTag("storage.name", storageName);
        activity?.SetTag("Storage.lease_duration", duration.TotalSeconds);
        try
        {
            if (!VKStorageGuard.IsValidSafePath(storageName, out var error))
            {
                return VKResult.Failure<VKStorageLeaseInfo>(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            var BlobClient = GetBlobClient(storageName);
            var leaseClient = BlobClient.GetBlobLeaseClient();
            var response = await leaseClient.AcquireAsync(duration, cancellationToken: cancellationToken);
            return VKResult.Success(new VKStorageLeaseInfo(
                response.Value.LeaseId,
                response.Value.LeaseTime is null or -1 ? null : DateTimeOffset.UtcNow.AddSeconds((double)response.Value.LeaseTime.Value)));
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseAlreadyPresent")
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return VKResult.Failure<VKStorageLeaseInfo>(VKStorageErrors.LeaseAlreadyHeld);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogLeaseOperationFailure(ex, nameof(AcquireLeaseAsync), storageName, containerProvider.GetContainerName());
            return VKResult.Failure<VKStorageLeaseInfo>(VKStorageErrors.GeneralFailure);
        }
    }
    public async Task<VKResult> ReleaseLeaseAsync(string storageName, string leaseId, CancellationToken cancellationToken = default)
    {
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(ReleaseLeaseAsync));
        activity?.SetTag("storage.name", storageName);
        activity?.SetTag("Storage.lease_id", leaseId);
        try
        {
            if (!VKStorageGuard.IsValidSafePath(storageName, out var error))
            {
                return VKResult.Failure(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            var BlobClient = GetBlobClient(storageName);
            var leaseClient = BlobClient.GetBlobLeaseClient(leaseId);
            await leaseClient.ReleaseAsync(cancellationToken: cancellationToken);
            return VKResult.Success();
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseIdMismatchWithLeaseOperation")
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return VKResult.Failure(VKStorageErrors.LeaseNotFound);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogLeaseOperationFailure(ex, nameof(ReleaseLeaseAsync), storageName, containerProvider.GetContainerName());
            return VKResult.Failure(VKStorageErrors.GeneralFailure);
        }
    }
    public async Task<VKResult<VKStorageLeaseInfo>> RenewLeaseAsync(string storageName, string leaseId, CancellationToken cancellationToken = default)
    {
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(RenewLeaseAsync));
        activity?.SetTag("storage.name", storageName);
        activity?.SetTag("Storage.lease_id", leaseId);
        try
        {
            if (!VKStorageGuard.IsValidSafePath(storageName, out var error))
            {
                return VKResult.Failure<VKStorageLeaseInfo>(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            var BlobClient = GetBlobClient(storageName);
            var leaseClient = BlobClient.GetBlobLeaseClient(leaseId);
            var response = await leaseClient.RenewAsync(cancellationToken: cancellationToken);
            return VKResult.Success(new VKStorageLeaseInfo(
                response.Value.LeaseId,
                null)); // Renew response doesn't always provide explicit time
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogLeaseOperationFailure(ex, nameof(RenewLeaseAsync), storageName, containerProvider.GetContainerName());
            return VKResult.Failure<VKStorageLeaseInfo>(VKStorageErrors.GeneralFailure);
        }
    }
    public async Task<VKResult> BreakLeaseAsync(string storageName, CancellationToken cancellationToken = default)
    {
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(BreakLeaseAsync));
        activity?.SetTag("storage.name", storageName);
        try
        {
            if (!VKStorageGuard.IsValidSafePath(storageName, out var error))
            {
                return VKResult.Failure(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            var BlobClient = GetBlobClient(storageName);
            var leaseClient = BlobClient.GetBlobLeaseClient();
            await leaseClient.BreakAsync(cancellationToken: cancellationToken);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogLeaseOperationFailure(ex, nameof(BreakLeaseAsync), storageName, containerProvider.GetContainerName());
            return VKResult.Failure(VKStorageErrors.GeneralFailure);
        }
    }
    private BlobClient GetBlobClient(string storageName)
    {
        var containerName = containerProvider.GetContainerName();
        return BlobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(storageName);
    }
}
