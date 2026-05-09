using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Storage.Azure.Diagnostics.Internal;

namespace VK.Blocks.Storage.Azure.Internal.Services;

internal sealed class StorageDirectoryService(
    IVKStorageContainerProvider containerProvider,
    BlobServiceClient blobServiceClient,
    ILogger<StorageDirectoryService> logger) : IVKStorageDirectoryService
{
    private readonly IVKStorageContainerProvider _containerProvider = VKGuard.NotNull(containerProvider);
    private readonly BlobServiceClient _blobServiceClient = VKGuard.NotNull(blobServiceClient);
    private readonly ILogger _logger = VKGuard.NotNull(logger);
    public async Task<VKResult> CreateDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(directoryPath);
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(CreateDirectoryAsync));
        activity?.SetTag("storage.directory_path", directoryPath);
        try
        {
            VKStorageGuard.EnsureValidDirectory(directoryPath);
            var containerClient = await GetContainerClientAsync(cancellationToken).ConfigureAwait(false);
            // 1. Check for name conflict with existing blobs (Case-Insensitive)
            var storageName = directoryPath.TrimEnd('/');
            await foreach (var blob in containerClient.GetBlobsAsync(prefix: storageName, cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                if (string.Equals(blob.Name, storageName, StringComparison.OrdinalIgnoreCase))
                {
                    return VKResult.Failure(VKStorageErrors.NameConflict);
                }
                break;
            }
            // 2. Ensure path ends with / and append marker
            var normalizedPath = storageName + "/";
            var markerPath = normalizedPath + VKStorageConstants.DirectoryMarker;
            var blobClient = containerClient.GetBlobClient(markerPath);
            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = VKStorageConstants.DirectoryContentType },
                Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { VKStorageConstants.DirectoryMetadataKey, VKStorageConstants.DirectoryMetadataValue }
                }
            };
            await blobClient.UploadAsync(BinaryData.FromBytes([]), options, cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogDirectoryOperationFailure(ex, nameof(CreateDirectoryAsync), directoryPath, _containerProvider.GetContainerName());
            return VKResult.Failure(VKStorageErrors.GeneralFailure);
        }
    }
    public async Task<VKResult> DeleteDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(directoryPath);
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(DeleteDirectoryAsync));
        activity?.SetTag("storage.directory_path", directoryPath);
        try
        {
            VKStorageGuard.EnsureValidDirectory(directoryPath);
            var containerClient = await GetContainerClientAsync(cancellationToken).ConfigureAwait(false);
            var prefix = directoryPath.TrimEnd('/') + "/";
            var batchClient = _blobServiceClient.GetBlobBatchClient();
            var blobsToDelete = new List<Uri>();
            await foreach (var blob in containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                blobsToDelete.Add(containerClient.GetBlobClient(blob.Name).Uri);
                // Azure Storage Batch limits to 256 items per request
                if (blobsToDelete.Count >= 256)
                {
                    await batchClient.DeleteBlobsAsync(blobsToDelete, cancellationToken: cancellationToken).ConfigureAwait(false);
                    blobsToDelete.Clear();
                }
            }
            if (blobsToDelete.Count > 0)
            {
                await batchClient.DeleteBlobsAsync(blobsToDelete, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogDirectoryOperationFailure(ex, nameof(DeleteDirectoryAsync), directoryPath, _containerProvider.GetContainerName());
            return VKResult.Failure(VKStorageErrors.GeneralFailure);
        }
    }
    public async Task<VKResult<bool>> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(directoryPath);
        try
        {
            var containerClient = await GetContainerClientAsync(cancellationToken).ConfigureAwait(false);
            var markerPath = directoryPath.TrimEnd('/') + "/" + VKStorageConstants.DirectoryMarker;
            var blobClient = containerClient.GetBlobClient(markerPath);
            var response = await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success(response.Value);
        }
        catch (Exception ex)
        {
            _logger.LogDirectoryOperationFailure(ex, nameof(DirectoryExistsAsync), directoryPath, _containerProvider.GetContainerName());
            return VKResult.Failure<bool>(VKStorageErrors.GeneralFailure);
        }
    }
    public async Task<VKResult<IReadOnlyList<VKStorageEntry>>> ListHierarchyAsync(
        string? prefix = null,
        string delimiter = "/",
        CancellationToken cancellationToken = default)
    {
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(ListHierarchyAsync));
        activity?.SetTag("storage.prefix", prefix);
        activity?.SetTag("storage.delimiter", delimiter);
        try
        {
            var containerClient = await GetContainerClientAsync(cancellationToken).ConfigureAwait(false);
            var items = new List<VKStorageEntry>();
            // Normalize prefix
            if (!string.IsNullOrEmpty(prefix))
            {
                prefix = prefix.TrimEnd('/') + delimiter;
            }
            var hierarchy = containerClient.GetBlobsByHierarchyAsync(
                BlobTraits.Metadata,
                BlobStates.None,
                prefix: prefix,
                delimiter: delimiter,
                cancellationToken: cancellationToken);
            await foreach (var item in hierarchy.ConfigureAwait(false))
            {
                if (item.IsPrefix)
                {
                    var folderName = ExtractName(item.Prefix, prefix, delimiter);
                    items.Add(new VKStorageEntry(folderName, item.Prefix, true));
                }
                else if (item.IsBlob)
                {
                    if (item.Blob.Name.EndsWith(VKStorageConstants.DirectoryMarker))
                    {
                        continue;
                    }
                    var fileName = ExtractName(item.Blob.Name, prefix, delimiter);
                    items.Add(new VKStorageEntry(
                        fileName,
                        item.Blob.Name,
                        false,
                        item.Blob.Properties.ContentLength,
                        item.Blob.Properties.ContentType,
                        item.Blob.Properties.CreatedOn,
                        item.Blob.Metadata));
                }
            }
            return VKResult.Success<IReadOnlyList<VKStorageEntry>>(items);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogDirectoryOperationFailure(ex, nameof(ListHierarchyAsync), prefix ?? string.Empty, _containerProvider.GetContainerName());
            return VKResult.Failure<IReadOnlyList<VKStorageEntry>>(VKStorageErrors.GeneralFailure);
        }
    }
    private async Task<BlobContainerClient> GetContainerClientAsync(CancellationToken cancellationToken)
    {
        var containerName = _containerProvider.GetContainerName();
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return containerClient;
    }
    private string ExtractName(string fullPath, string? prefix, string delimiter)
    {
        if (string.IsNullOrEmpty(prefix))
            return fullPath.TrimEnd(delimiter[0]);
        var name = fullPath.Substring(prefix.Length);
        return name.TrimEnd(delimiter[0]);
    }
}
