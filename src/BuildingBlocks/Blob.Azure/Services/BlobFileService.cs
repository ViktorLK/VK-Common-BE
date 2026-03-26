using System.Diagnostics;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Blob.Abstractions;
using VK.Blocks.Blob.Abstractions.Contracts;
using VK.Blocks.Blob.Constants;
using VK.Blocks.Blob.Guards;
using VK.Blocks.Blob.Options;
using VK.Blocks.Blob.Exceptions;
using VK.Blocks.Blob.Diagnostics;
using VK.Blocks.Core.Abstractions;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Blob.Services;

public sealed class BlobFileService(
    IBlobContainerProvider containerProvider,
    BlobServiceClient blobServiceClient,
    IOptions<BlobOptions> options,
    ILogger<BlobFileService> logger) : IBlobFileService, IBlobUriGenerator
{
    private readonly BlobOptions _options = options.Value;

    public async Task<Result<string>> UploadAsync(
        BlobUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(UploadAsync));
        activity?.SetTag("blob.name", request.FileName);
        activity?.SetTag("blob.container", containerProvider.GetContainerName());

        try
        {
            BlobGuard.EnsureValidUpload(request, _options);

            var containerClient = await GetContainerClientAsync(cancellationToken);

            // 1. Check for name conflict with existing virtual directories (Case-Insensitive)
            var blobName = request.FileName.TrimEnd('/');
            var prefix = blobName + "/";
            
            // Check if it's a directory
            await foreach (var blob in containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
            {
                if (blob.Name.EndsWith(BlobConstants.DirectoryMarker, StringComparison.OrdinalIgnoreCase))
                {
                    return Result.Failure<string>(BlobErrors.NameConflict);
                }
                break;
            }

            // 2. Check for name conflict with existing files (Case-Insensitive)
            // Since Azure Blob is case-sensitive, we list blobs with the same name prefix and check manually
            await foreach (var blob in containerClient.GetBlobsAsync(prefix: blobName, cancellationToken: cancellationToken))
            {
                if (string.Equals(blob.Name, blobName, StringComparison.OrdinalIgnoreCase))
                {
                    return Result.Failure<string>(BlobErrors.NameConflict);
                }
                break;
            }

            var blobClient = containerClient.GetBlobClient(request.FileName);

            // 3. Prepare Metadata
            var metadata = request.Metadata ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Add internal infrastructure metadata
            metadata[BlobConstants.DirectoryMetadataKey] = "File";

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = request.ContentType },
                Metadata = metadata
            };

            await blobClient.UploadAsync(request.Content, uploadOptions, cancellationToken);

            return Result.Success(request.FileName);
        }
        catch (BlobValidationException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return Result.Failure<string>(new Error(ex.Code, ex.Message, ErrorType.Validation));
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to upload blob {FileName} to Azure Storage.", request.FileName);
            return Result.Failure<string>(BlobErrors.UploadFailed);
        }
    }

    public async Task<Result<BlobDownloadResponse>> DownloadAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(DownloadAsync));
        activity?.SetTag("blob.name", blobName);
        activity?.SetTag("blob.container", containerProvider.GetContainerName());

        try
        {
            if (!BlobGuard.IsValidSafePath(blobName, out var error))
            {
                return Result.Failure<BlobDownloadResponse>(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            var containerClient = await GetContainerClientAsync(cancellationToken);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                return Result.Failure<BlobDownloadResponse>(BlobErrors.NotFound);
            }

            var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);

            return Result.Success(new BlobDownloadResponse(
                response.Value.Content,
                response.Value.Details.ContentType,
                blobName));
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to download blob {BlobName} from Azure Storage.", blobName);
            return Result.Failure<BlobDownloadResponse>(BlobErrors.GeneralFailure);
        }
    }

    public async Task<Result<BlobDownloadResponse>> DownloadVersionAsync(
        string blobName,
        string versionId,
        CancellationToken cancellationToken = default)
    {
        if (!_options.EnableVersioning)
        {
            return Result.Failure<BlobDownloadResponse>(BlobErrors.FeatureDisabled);
        }

        try
        {
            if (!BlobGuard.IsValidSafePath(blobName, out var error))
            {
                return Result.Failure<BlobDownloadResponse>(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            var containerClient = await GetContainerClientAsync(cancellationToken);
            var blobClient = containerClient.GetBlobClient(blobName).WithVersion(versionId);

            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                return Result.Failure<BlobDownloadResponse>(BlobErrors.VersionNotFound);
            }

            var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);

            return Result.Success(new BlobDownloadResponse(
                response.Value.Content,
                response.Value.Details.ContentType,
                blobName));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to download version {VersionId} of blob {BlobName}.", versionId, blobName);
            return Result.Failure<BlobDownloadResponse>(BlobErrors.GeneralFailure);
        }
    }

    public async Task<Result> DeleteAsync(
        string blobName,
        BlobRemoveOptions? deleteOptions = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(DeleteAsync));
        activity?.SetTag("blob.name", blobName);
        activity?.SetTag("blob.container", containerProvider.GetContainerName());
        activity?.SetTag("blob.delete_mode", deleteOptions?.Mode.ToString());

        try
        {
            if (!BlobGuard.IsValidSafePath(blobName, out var error))
            {
                return Result.Failure(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            deleteOptions ??= new BlobRemoveOptions();

            if (deleteOptions.Mode == BlobDeleteMode.SoftDelete && !_options.EnableSoftDelete)
            {
                return Result.Failure(BlobErrors.FeatureDisabled);
            }

            var containerClient = await GetContainerClientAsync(cancellationToken);
            var blobClient = containerClient.GetBlobClient(blobName);

            var snapshotsOption = (deleteOptions.Mode == BlobDeleteMode.PermanentDelete || deleteOptions.IncludeSnapshots)
                ? DeleteSnapshotsOption.IncludeSnapshots
                : DeleteSnapshotsOption.None;

            await blobClient.DeleteIfExistsAsync(snapshotsOption, cancellationToken: cancellationToken);

            logger.LogInformation("Deleted blob {BlobName} from Azure Storage. Mode: {DeleteMode}", blobName, deleteOptions.Mode);

            return Result.Success();
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to delete blob {BlobName} from Azure Storage.", blobName);
            return Result.Failure(BlobErrors.GeneralFailure);
        }
    }

    public async Task<Result> UndeleteAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        if (!_options.EnableSoftDelete)
        {
            return Result.Failure(BlobErrors.FeatureDisabled);
        }

        try
        {
            if (!BlobGuard.IsValidSafePath(blobName, out var error))
            {
                return Result.Failure(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            var containerClient = await GetContainerClientAsync(cancellationToken);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.UndeleteAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to undelete blob {BlobName}.", blobName);
            return Result.Failure(BlobErrors.UndeleteFailed);
        }
    }

    public async Task<Result<bool>> ExistsAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!BlobGuard.IsValidSafePath(blobName, out var error))
            {
                return Result.Failure<bool>(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            var containerClient = await GetContainerClientAsync(cancellationToken);
            var blobClient = containerClient.GetBlobClient(blobName);
            var exists = await blobClient.ExistsAsync(cancellationToken);
            return Result.Success(exists.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check if blob {BlobName} exists in Azure Storage.", blobName);
            return Result.Failure<bool>(BlobErrors.GeneralFailure);
        }
    }

    public async Task<Result<BlobFileMetadata>> GetInfoAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!BlobGuard.IsValidSafePath(blobName, out var error))
            {
                return Result.Failure<BlobFileMetadata>(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            var containerClient = await GetContainerClientAsync(cancellationToken);
            var blobClient = containerClient.GetBlobClient(blobName);

            var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

            return Result.Success(new BlobFileMetadata(
                blobName,
                properties.Value.ContentLength,
                properties.Value.ContentType,
                properties.Value.CreatedOn,
                properties.Value.Metadata));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get info for blob {BlobName} from Azure Storage.", blobName);
            return Result.Failure<BlobFileMetadata>(BlobErrors.GeneralFailure);
        }
    }

    public async Task<Result<IReadOnlyList<string>>> ListVersionsAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        if (!_options.EnableVersioning)
        {
            return Result.Failure<IReadOnlyList<string>>(BlobErrors.FeatureDisabled);
        }

        try
        {
            if (!BlobGuard.IsValidSafePath(blobName, out var error))
            {
                return Result.Failure<IReadOnlyList<string>>(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            var containerClient = await GetContainerClientAsync(cancellationToken);
            var versions = new List<string>();

            await foreach (var item in containerClient.GetBlobsAsync(
                BlobTraits.None,
                BlobStates.Version,
                prefix: blobName,
                cancellationToken: cancellationToken))
            {
                if (item.Name == blobName && !string.IsNullOrEmpty(item.VersionId))
                {
                    versions.Add(item.VersionId);
                }
            }

            return Result.Success<IReadOnlyList<string>>(versions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list versions for blob {BlobName}.", blobName);
            return Result.Failure<IReadOnlyList<string>>(BlobErrors.GeneralFailure);
        }
    }

    public Result<string> GenerateSasUri(string blobName, BlobSasOptions sasOptions)
    {
        try
        {
            var containerName = containerProvider.GetContainerName();
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (!blobClient.CanGenerateSasUri)
            {
                return Result.Failure<string>(BlobErrors.GeneralFailure);
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.Add(sasOptions.ExpiresIn)
            };

            BlobSasPermissions permissions = 0;
            if (sasOptions.Permissions.HasFlag(BlobPermissions.Read))
                permissions |= BlobSasPermissions.Read;
            if (sasOptions.Permissions.HasFlag(BlobPermissions.Write))
                permissions |= BlobSasPermissions.Write;
            if (sasOptions.Permissions.HasFlag(BlobPermissions.Delete))
                permissions |= BlobSasPermissions.Delete;

            sasBuilder.SetPermissions(permissions);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return Result.Success(sasUri.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate SAS URI for blob {BlobName}.", blobName);
            return Result.Failure<string>(BlobErrors.GeneralFailure);
        }
    }

    public Result<string> GetPublicUrl(string blobName)
    {
        var containerName = containerProvider.GetContainerName();
        var blobClient = blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
        return Result.Success(blobClient.Uri.ToString());
    }

    private async Task<BlobContainerClient> GetContainerClientAsync(CancellationToken cancellationToken)
    {
        var containerName = containerProvider.GetContainerName();
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        return containerClient;
    }
}
