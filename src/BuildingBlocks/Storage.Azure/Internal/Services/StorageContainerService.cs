using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Storage.Azure.Diagnostics.Internal;

namespace VK.Blocks.Storage.Azure.Internal.Services;

internal sealed class StorageContainerService(
    BlobServiceClient BlobServiceClient,
    ILogger<StorageContainerService> logger) : IVKStorageContainerService
{
    public async Task<VKResult> CreateContainerAsync(string containerName, CancellationToken cancellationToken = default)
    {
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(CreateContainerAsync));
        activity?.SetTag("storage.container", containerName);
        try
        {
            if (!VKStorageGuard.IsValidSafePath(containerName, out var error))
            {
                return VKResult.Failure(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            var containerClient = BlobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            logger.LogContainerOperationFailure(ex, nameof(CreateContainerAsync), containerName);
            return VKResult.Failure(VKStorageErrors.GeneralFailure);
        }
    }
    public async Task<VKResult> DeleteContainerAsync(string containerName, CancellationToken cancellationToken = default)
    {
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(DeleteContainerAsync));
        activity?.SetTag("storage.container", containerName);
        try
        {
            if (!VKStorageGuard.IsValidSafePath(containerName, out var error))
            {
                return VKResult.Failure(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            var containerClient = BlobServiceClient.GetBlobContainerClient(containerName);
            var response = await containerClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            return response.Value
                ? VKResult.Success()
                : VKResult.Failure(VKStorageErrors.ContainerNotFound);
        }
        catch (Exception ex)
        {
            logger.LogContainerOperationFailure(ex, nameof(DeleteContainerAsync), containerName);
            return VKResult.Failure(VKStorageErrors.GeneralFailure);
        }
    }
    public async Task<VKResult<bool>> ContainerExistsAsync(string containerName, CancellationToken cancellationToken = default)
    {
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(ContainerExistsAsync));
        activity?.SetTag("storage.container", containerName);
        try
        {
            if (!VKStorageGuard.IsValidSafePath(containerName, out var error))
            {
                return VKResult.Failure<bool>(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            var containerClient = BlobServiceClient.GetBlobContainerClient(containerName);
            var response = await containerClient.ExistsAsync(cancellationToken);
            return VKResult.Success(response.Value);
        }
        catch (Exception ex)
        {
            logger.LogContainerOperationFailure(ex, nameof(ContainerExistsAsync), containerName);
            return VKResult.Failure<bool>(VKStorageErrors.GeneralFailure);
        }
    }
    public async Task<VKResult<IReadOnlyList<string>>> ListContainersAsync(string? prefix = null, CancellationToken cancellationToken = default)
    {
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(ListContainersAsync));
        activity?.SetTag("storage.prefix", prefix);
        try
        {
            var containers = new List<string>();
            await foreach (var item in BlobServiceClient.GetBlobContainersAsync(prefix: prefix, cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                containers.Add(item.Name);
            }
            return VKResult.Success<IReadOnlyList<string>>(containers);
        }
        catch (Exception ex)
        {
            logger.LogContainerOperationFailure(ex, nameof(ListContainersAsync), prefix ?? string.Empty);
            return VKResult.Failure<IReadOnlyList<string>>(VKStorageErrors.GeneralFailure);
        }
    }
}
