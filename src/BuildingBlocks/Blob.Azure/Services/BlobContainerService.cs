using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using VK.Blocks.Blob.Abstractions;
using VK.Blocks.Blob.Constants;
using VK.Blocks.Blob.Guards;
using VK.Blocks.Core.Results;

using VK.Blocks.Blob.Diagnostics;

namespace VK.Blocks.Blob.Services;

public sealed class BlobContainerService(
    BlobServiceClient blobServiceClient,
    ILogger<BlobContainerService> logger) : IBlobContainerService
{
    public async Task<Result> CreateContainerAsync(string containerName, CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(CreateContainerAsync));
        activity?.SetTag("blob.container", containerName);

        try
        {
            if (!BlobGuard.IsValidSafePath(containerName, out var error))
            {
                return Result.Failure(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create container {ContainerName}.", containerName);
            return Result.Failure(BlobErrors.GeneralFailure);
        }
    }

    public async Task<Result> DeleteContainerAsync(string containerName, CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(DeleteContainerAsync));
        activity?.SetTag("blob.container", containerName);

        try
        {
            if (!BlobGuard.IsValidSafePath(containerName, out var error))
            {
                return Result.Failure(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var response = await containerClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

            return response.Value
                ? Result.Success()
                : Result.Failure(BlobErrors.ContainerNotFound);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete container {ContainerName}.", containerName);
            return Result.Failure(BlobErrors.GeneralFailure);
        }
    }

    public async Task<Result<bool>> ContainerExistsAsync(string containerName, CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(ContainerExistsAsync));
        activity?.SetTag("blob.container", containerName);

        try
        {
            if (!BlobGuard.IsValidSafePath(containerName, out var error))
            {
                return Result.Failure<bool>(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var response = await containerClient.ExistsAsync(cancellationToken);
            return Result.Success(response.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check if container {ContainerName} exists.", containerName);
            return Result.Failure<bool>(BlobErrors.GeneralFailure);
        }
    }

    public async Task<Result<IReadOnlyList<string>>> ListContainersAsync(string? prefix = null, CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(ListContainersAsync));
        activity?.SetTag("blob.prefix", prefix);

        try
        {
            var containers = new List<string>();
            await foreach (var item in blobServiceClient.GetBlobContainersAsync(prefix: prefix, cancellationToken: cancellationToken))
            {
                containers.Add(item.Name);
            }
            return Result.Success<IReadOnlyList<string>>(containers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list containers with prefix {Prefix}.", prefix);
            return Result.Failure<IReadOnlyList<string>>(BlobErrors.GeneralFailure);
        }
    }
}
