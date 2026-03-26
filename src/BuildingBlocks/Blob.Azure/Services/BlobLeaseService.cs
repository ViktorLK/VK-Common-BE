using System.Diagnostics;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;
using VK.Blocks.Blob.Abstractions;
using VK.Blocks.Blob.Abstractions.Contracts;
using VK.Blocks.Blob.Constants;
using VK.Blocks.Blob.Guards;
using VK.Blocks.Blob.Diagnostics;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Blob.Services;

public sealed class BlobLeaseService(
    IBlobContainerProvider containerProvider,
    BlobServiceClient blobServiceClient,
    ILogger<BlobLeaseService> logger) : IBlobLeaseService
{
    public async Task<Result<BlobLeaseInfo>> AcquireLeaseAsync(string blobName, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(AcquireLeaseAsync));
        activity?.SetTag("blob.name", blobName);
        activity?.SetTag("blob.lease_duration", duration.TotalSeconds);

        try
        {
            if (!BlobGuard.IsValidSafePath(blobName, out var error))
            {
                return Result.Failure<BlobLeaseInfo>(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            var blobClient = GetBlobClient(blobName);
            var leaseClient = blobClient.GetBlobLeaseClient();

            var response = await leaseClient.AcquireAsync(duration, cancellationToken: cancellationToken);

            return Result.Success(new BlobLeaseInfo(
                response.Value.LeaseId,
                response.Value.LeaseTime is null or -1 ? null : DateTimeOffset.UtcNow.AddSeconds((double)response.Value.LeaseTime.Value)));
        }
        catch (Azure.RequestFailedException ex) when (ex.ErrorCode == "LeaseAlreadyPresent")
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return Result.Failure<BlobLeaseInfo>(BlobErrors.LeaseAlreadyHeld);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to acquire lease for blob {BlobName}.", blobName);
            return Result.Failure<BlobLeaseInfo>(BlobErrors.GeneralFailure);
        }
    }

    public async Task<Result> ReleaseLeaseAsync(string blobName, string leaseId, CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(ReleaseLeaseAsync));
        activity?.SetTag("blob.name", blobName);
        activity?.SetTag("blob.lease_id", leaseId);

        try
        {
            if (!BlobGuard.IsValidSafePath(blobName, out var error))
            {
                return Result.Failure(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            var blobClient = GetBlobClient(blobName);
            var leaseClient = blobClient.GetBlobLeaseClient(leaseId);

            await leaseClient.ReleaseAsync(cancellationToken: cancellationToken);
            return Result.Success();
        }
        catch (Azure.RequestFailedException ex) when (ex.ErrorCode == "LeaseIdMismatchWithLeaseOperation")
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return Result.Failure(BlobErrors.LeaseNotFound);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to release lease for blob {BlobName}.", blobName);
            return Result.Failure(BlobErrors.GeneralFailure);
        }
    }

    public async Task<Result<BlobLeaseInfo>> RenewLeaseAsync(string blobName, string leaseId, CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(RenewLeaseAsync));
        activity?.SetTag("blob.name", blobName);
        activity?.SetTag("blob.lease_id", leaseId);

        try
        {
            if (!BlobGuard.IsValidSafePath(blobName, out var error))
            {
                return Result.Failure<BlobLeaseInfo>(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            var blobClient = GetBlobClient(blobName);
            var leaseClient = blobClient.GetBlobLeaseClient(leaseId);

            var response = await leaseClient.RenewAsync(cancellationToken: cancellationToken);

            return Result.Success(new BlobLeaseInfo(
                response.Value.LeaseId,
                null)); // Renew response doesn't always provide explicit time
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to renew lease for blob {BlobName}.", blobName);
            return Result.Failure<BlobLeaseInfo>(BlobErrors.GeneralFailure);
        }
    }

    public async Task<Result> BreakLeaseAsync(string blobName, CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(BreakLeaseAsync));
        activity?.SetTag("blob.name", blobName);

        try
        {
            if (!BlobGuard.IsValidSafePath(blobName, out var error))
            {
                return Result.Failure(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            var blobClient = GetBlobClient(blobName);
            var leaseClient = blobClient.GetBlobLeaseClient();

            await leaseClient.BreakAsync(cancellationToken: cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to break lease for blob {BlobName}.", blobName);
            return Result.Failure(BlobErrors.GeneralFailure);
        }
    }

    private BlobClient GetBlobClient(string blobName)
    {
        var containerName = containerProvider.GetContainerName();
        return blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
    }
}
