using System.Diagnostics;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs.Batch;
using Microsoft.Extensions.Logging;
using VK.Blocks.Blob.Abstractions;
using VK.Blocks.Blob.Abstractions.Contracts;
using VK.Blocks.Blob.Constants;
using VK.Blocks.Blob.Guards;
using VK.Blocks.Blob.Diagnostics;
using VK.Blocks.Core.Abstractions;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Blob.Services;

public sealed class BlobDirectoryService(
    IBlobContainerProvider containerProvider,
    BlobServiceClient blobServiceClient,
    ILogger<BlobDirectoryService> logger) : IBlobDirectoryService
{
    public async Task<Result> CreateDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(CreateDirectoryAsync));
        activity?.SetTag("blob.directory_path", directoryPath);

        try
        {
            BlobGuard.EnsureValidDirectory(directoryPath);
            var containerClient = GetContainerClient();
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            // 1. Check for name conflict with existing blobs (Case-Insensitive)
            var blobName = directoryPath.TrimEnd('/');
            await foreach (var blob in containerClient.GetBlobsAsync(prefix: blobName, cancellationToken: cancellationToken))
            {
                if (string.Equals(blob.Name, blobName, StringComparison.OrdinalIgnoreCase))
                {
                    return Result.Failure(BlobErrors.NameConflict);
                }
                break;
            }

            // 2. Ensure path ends with / and append marker
            var normalizedPath = blobName + "/";
            var markerPath = normalizedPath + BlobConstants.DirectoryMarker;

            var blobClient = containerClient.GetBlobClient(markerPath);

            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = BlobConstants.DirectoryContentType },
                Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { BlobConstants.DirectoryMetadataKey, BlobConstants.DirectoryMetadataValue }
                }
            };

            await blobClient.UploadAsync(BinaryData.FromBytes([]), options, cancellationToken: cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to create directory {DirectoryPath}.", directoryPath);
            return Result.Failure(BlobErrors.GeneralFailure);
        }
    }

    public async Task<Result> DeleteDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(DeleteDirectoryAsync));
        activity?.SetTag("blob.directory_path", directoryPath);

        try
        {
            BlobGuard.EnsureValidDirectory(directoryPath);
            var containerClient = GetContainerClient();
            var prefix = directoryPath.TrimEnd('/') + "/";

            var batchClient = containerClient.GetParentBlobServiceClient().GetBlobBatchClient();
            var blobsToDelete = new List<Uri>();

            await foreach (var blob in containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
            {
                blobsToDelete.Add(containerClient.GetBlobClient(blob.Name).Uri);

                // Azure Blob Batch limits to 256 items per request
                if (blobsToDelete.Count >= 256)
                {
                    await batchClient.DeleteBlobsAsync(blobsToDelete, cancellationToken: cancellationToken);
                    blobsToDelete.Clear();
                }
            }

            if (blobsToDelete.Count > 0)
            {
                await batchClient.DeleteBlobsAsync(blobsToDelete, cancellationToken: cancellationToken);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to delete directory {DirectoryPath}.", directoryPath);
            return Result.Failure(BlobErrors.GeneralFailure);
        }
    }

    public async Task<Result<bool>> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = GetContainerClient();
            var markerPath = directoryPath.TrimEnd('/') + "/" + BlobConstants.DirectoryMarker;
            var blobClient = containerClient.GetBlobClient(markerPath);

            var response = await blobClient.ExistsAsync(cancellationToken);
            return Result.Success(response.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check if directory {Path} exists.", directoryPath);
            return Result.Failure<bool>(BlobErrors.GeneralFailure);
        }
    }

    public async Task<Result<IReadOnlyList<BlobEntry>>> ListHierarchyAsync(
        string? prefix = null,
        string delimiter = "/",
        CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(ListHierarchyAsync));
        activity?.SetTag("blob.prefix", prefix);
        activity?.SetTag("blob.delimiter", delimiter);

        try
        {
            var containerClient = GetContainerClient();
            var items = new List<BlobEntry>();

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

            await foreach (var item in hierarchy)
            {
                if (item.IsPrefix)
                {
                    var folderName = ExtractName(item.Prefix, prefix, delimiter);
                    items.Add(new BlobEntry(folderName, item.Prefix, true));
                }
                else if (item.IsBlob)
                {
                    if (item.Blob.Name.EndsWith(BlobConstants.DirectoryMarker))
                    {
                        continue;
                    }

                    var fileName = ExtractName(item.Blob.Name, prefix, delimiter);
                    items.Add(new BlobEntry(
                        fileName,
                        item.Blob.Name,
                        false,
                        item.Blob.Properties.ContentLength,
                        item.Blob.Properties.ContentType,
                        item.Blob.Properties.CreatedOn,
                        item.Blob.Metadata));
                }
            }

            return Result.Success<IReadOnlyList<BlobEntry>>(items);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to list directory hierarchy with prefix {Prefix}.", prefix);
            return Result.Failure<IReadOnlyList<BlobEntry>>(BlobErrors.GeneralFailure);
        }
    }

    private BlobContainerClient GetContainerClient()
    {
        var containerName = containerProvider.GetContainerName();
        return blobServiceClient.GetBlobContainerClient(containerName);
    }

    private string ExtractName(string fullPath, string? prefix, string delimiter)
    {
        if (string.IsNullOrEmpty(prefix))
            return fullPath.TrimEnd(delimiter[0]);
        var name = fullPath.Substring(prefix.Length);
        return name.TrimEnd(delimiter[0]);
    }
}
