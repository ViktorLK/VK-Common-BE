using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Storage.Azure.Diagnostics.Internal;

namespace VK.Blocks.Storage.Azure.Internal.Services;

internal sealed class StorageTagService(
    IVKStorageContainerProvider containerProvider,
    BlobServiceClient BlobServiceClient,
    ILogger<StorageTagService> logger) : IVKStorageTagService
{
    public async Task<VKResult> SetTagsAsync(string storageName, IDictionary<string, string> tags, CancellationToken cancellationToken = default)
    {
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(SetTagsAsync));
        activity?.SetTag("storage.name", storageName);
        activity?.SetTag("storage.container", containerProvider.GetContainerName());
        try
        {
            if (!VKStorageGuard.IsValidSafePath(storageName, out var error))
            {
                return VKResult.Failure(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            var BlobClient = GetBlobClient(storageName);
            await BlobClient.SetTagsAsync(tags, cancellationToken: cancellationToken);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            logger.LogTagOperationFailure(ex, nameof(SetTagsAsync), storageName, containerProvider.GetContainerName());
            return VKResult.Failure(VKStorageErrors.TagOperationFailed);
        }
    }
    public async Task<VKResult<IDictionary<string, string>>> GetTagsAsync(string storageName, CancellationToken cancellationToken = default)
    {
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(GetTagsAsync));
        activity?.SetTag("storage.name", storageName);
        activity?.SetTag("storage.container", containerProvider.GetContainerName());
        try
        {
            if (!VKStorageGuard.IsValidSafePath(storageName, out var error))
            {
                return VKResult.Failure<IDictionary<string, string>>(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            var BlobClient = GetBlobClient(storageName);
            var response = await BlobClient.GetTagsAsync(cancellationToken: cancellationToken);
            return VKResult.Success<IDictionary<string, string>>(response.Value.Tags);
        }
        catch (Exception ex)
        {
            logger.LogTagOperationFailure(ex, nameof(GetTagsAsync), storageName, containerProvider.GetContainerName());
            return VKResult.Failure<IDictionary<string, string>>(VKStorageErrors.GeneralFailure);
        }
    }
    public async Task<VKResult> SetMetadataAsync(string storageName, IDictionary<string, string> metadata, CancellationToken cancellationToken = default)
    {
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(SetMetadataAsync));
        activity?.SetTag("storage.name", storageName);
        activity?.SetTag("storage.container", containerProvider.GetContainerName());
        try
        {
            if (!VKStorageGuard.IsValidSafePath(storageName, out var error))
            {
                return VKResult.Failure(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            var BlobClient = GetBlobClient(storageName);
            await BlobClient.SetMetadataAsync(metadata, cancellationToken: cancellationToken);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            logger.LogTagOperationFailure(ex, nameof(SetMetadataAsync), storageName, containerProvider.GetContainerName());
            return VKResult.Failure(VKStorageErrors.TagOperationFailed);
        }
    }
    public async Task<VKResult<IDictionary<string, string>>> GetMetadataAsync(string storageName, CancellationToken cancellationToken = default)
    {
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(GetMetadataAsync));
        activity?.SetTag("storage.name", storageName);
        activity?.SetTag("storage.container", containerProvider.GetContainerName());
        try
        {
            if (!VKStorageGuard.IsValidSafePath(storageName, out var error))
            {
                return VKResult.Failure<IDictionary<string, string>>(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            var BlobClient = GetBlobClient(storageName);
            var properties = await BlobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            return VKResult.Success<IDictionary<string, string>>(properties.Value.Metadata);
        }
        catch (Exception ex)
        {
            logger.LogTagOperationFailure(ex, nameof(GetMetadataAsync), storageName, containerProvider.GetContainerName());
            return VKResult.Failure<IDictionary<string, string>>(VKStorageErrors.GeneralFailure);
        }
    }
    public async Task<VKResult<IReadOnlyList<string>>> FindStoragesByTagAsync(string tagFilterExpression, CancellationToken cancellationToken = default)
    {
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(FindStoragesByTagAsync));
        activity?.SetTag("storage.tag_filter", tagFilterExpression);
        try
        {
            var storages = new List<string>();
            await foreach (var item in BlobServiceClient.FindBlobsByTagsAsync(tagFilterExpression, cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                storages.Add(item.BlobName);
            }
            return VKResult.Success<IReadOnlyList<string>>(storages);
        }
        catch (Exception ex)
        {
            logger.LogTagOperationFailure(ex, nameof(FindStoragesByTagAsync), tagFilterExpression, containerProvider.GetContainerName());
            return VKResult.Failure<IReadOnlyList<string>>(VKStorageErrors.TagOperationFailed);
        }
    }
    private BlobClient GetBlobClient(string storageName)
    {
        var containerName = containerProvider.GetContainerName();
        return BlobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(storageName);
    }
}
