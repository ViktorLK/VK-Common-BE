using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.Storage.Azure.Diagnostics.Internal;

namespace VK.Blocks.Storage.Azure.Internal.Services;

internal sealed class StorageFileService(
    IVKStorageContainerProvider containerProvider,
    BlobServiceClient blobServiceClient,
    IOptions<VKStorageOptions> options,
    TimeProvider timeProvider,
    ILogger<StorageFileService> logger) : IVKStorageFileService, IVKStorageUriGenerator
{
    private readonly VKStorageOptions _options = VKGuard.NotNull(options).Value;
    private readonly IVKStorageContainerProvider _containerProvider = VKGuard.NotNull(containerProvider);
    private readonly BlobServiceClient _blobServiceClient = VKGuard.NotNull(blobServiceClient);
    private readonly TimeProvider _timeProvider = VKGuard.NotNull(timeProvider);
    private readonly ILogger _logger = VKGuard.NotNull(logger);
    public async Task<VKResult<string>> UploadAsync(
        VKStorageUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(request);
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(UploadAsync));
        var containerName = _containerProvider.GetContainerName();
        activity?.SetTag("storage.name", request.FileName);
        activity?.SetTag("storage.container", containerName);
        try
        {
            VKStorageGuard.EnsureValidUpload(request, _options);
            var containerClient = await GetContainerClientAsync(cancellationToken).ConfigureAwait(false);
            // 1. Check for name conflict with existing virtual directories (Case-Insensitive)
            var storageName = request.FileName.TrimEnd('/');
            var prefix = storageName + "/";
            await foreach (var blob in containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                if (blob.Name.EndsWith(VKStorageConstants.DirectoryMarker, StringComparison.OrdinalIgnoreCase))
                {
                    return VKResult.Failure<string>(VKStorageErrors.NameConflict);
                }
                break;
            }
            // 2. Check for name conflict with existing files (Case-Insensitive)
            await foreach (var blob in containerClient.GetBlobsAsync(prefix: storageName, cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                if (string.Equals(blob.Name, storageName, StringComparison.OrdinalIgnoreCase))
                {
                    return VKResult.Failure<string>(VKStorageErrors.NameConflict);
                }
                break;
            }
            var blobClient = containerClient.GetBlobClient(request.FileName);
            // 3. Prepare Metadata
            var metadata = request.Metadata ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            metadata[VKStorageConstants.DirectoryMetadataKey] = "File";
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = request.ContentType },
                Metadata = metadata
            };
            await blobClient.UploadAsync(request.Content, uploadOptions, cancellationToken).ConfigureAwait(false);
            _logger.LogUploadSuccess(request.FileName, containerName);
            return VKResult.Success(request.FileName);
        }
        catch (VKStorageValidationException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return VKResult.Failure<string>(new VKError(ex.Code, ex.Message, VKErrorType.Validation));
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogUploadFailure(ex, request.FileName, containerName);
            return VKResult.Failure<string>(VKStorageErrors.UploadFailed);
        }
    }
    public async Task<VKResult<VKStorageDownloadResponse>> DownloadAsync(
        string storageName,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(storageName);
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(DownloadAsync));
        var containerName = _containerProvider.GetContainerName();
        activity?.SetTag("storage.name", storageName);
        activity?.SetTag("storage.container", containerName);
        try
        {
            if (!VKStorageGuard.IsValidSafePath(storageName, out var error))
            {
                return VKResult.Failure<VKStorageDownloadResponse>(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            var containerClient = await GetContainerClientAsync(cancellationToken).ConfigureAwait(false);
            var blobClient = containerClient.GetBlobClient(storageName);
            if (!await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false))
            {
                return VKResult.Failure<VKStorageDownloadResponse>(VKStorageErrors.NotFound);
            }
            var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            return VKResult.Success(new VKStorageDownloadResponse(
                response.Value.Content,
                response.Value.Details.ContentType,
                storageName));
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogDownloadFailure(ex, storageName, containerName);
            return VKResult.Failure<VKStorageDownloadResponse>(VKStorageErrors.GeneralFailure);
        }
    }
    public async Task<VKResult<VKStorageDownloadResponse>> DownloadVersionAsync(
        string storageName,
        string versionId,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(storageName);
        VKGuard.NotNullOrWhiteSpace(versionId);
        if (!_options.EnableVersioning)
        {
            return VKResult.Failure<VKStorageDownloadResponse>(VKStorageErrors.FeatureDisabled);
        }
        try
        {
            if (!VKStorageGuard.IsValidSafePath(storageName, out var error))
            {
                return VKResult.Failure<VKStorageDownloadResponse>(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            var containerClient = await GetContainerClientAsync(cancellationToken).ConfigureAwait(false);
            var blobClient = containerClient.GetBlobClient(storageName).WithVersion(versionId);
            if (!await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false))
            {
                return VKResult.Failure<VKStorageDownloadResponse>(VKStorageErrors.VersionNotFound);
            }
            var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            return VKResult.Success(new VKStorageDownloadResponse(
                response.Value.Content,
                response.Value.Details.ContentType,
                storageName));
        }
        catch (Exception ex)
        {
            _logger.LogDownloadFailure(ex, storageName, _containerProvider.GetContainerName());
            return VKResult.Failure<VKStorageDownloadResponse>(VKStorageErrors.GeneralFailure);
        }
    }
    public async Task<VKResult> DeleteAsync(
        string storageName,
        VKStorageRemoveOptions? deleteOptions = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(storageName);
        using var activity = StorageAzureDiagnostics.Source.StartActivity(nameof(DeleteAsync));
        var containerName = _containerProvider.GetContainerName();
        activity?.SetTag("storage.name", storageName);
        activity?.SetTag("storage.container", containerName);
        activity?.SetTag("storage.delete_mode", deleteOptions?.Mode.ToString());
        try
        {
            if (!VKStorageGuard.IsValidSafePath(storageName, out var error))
            {
                return VKResult.Failure(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            deleteOptions ??= new VKStorageRemoveOptions();
            if (deleteOptions.Mode == VKStorageDeleteMode.SoftDelete && !_options.EnableSoftDelete)
            {
                return VKResult.Failure(VKStorageErrors.FeatureDisabled);
            }
            var containerClient = await GetContainerClientAsync(cancellationToken).ConfigureAwait(false);
            var blobClient = containerClient.GetBlobClient(storageName);
            var snapshotsOption = (deleteOptions.Mode == VKStorageDeleteMode.PermanentDelete || deleteOptions.IncludeSnapshots)
                ? DeleteSnapshotsOption.IncludeSnapshots
                : DeleteSnapshotsOption.None;
            await blobClient.DeleteIfExistsAsync(snapshotsOption, cancellationToken: cancellationToken).ConfigureAwait(false);
            _logger.LogDeleteSuccess(storageName, containerName, deleteOptions.Mode.ToString());
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogDeleteFailure(ex, storageName, containerName);
            return VKResult.Failure(VKStorageErrors.GeneralFailure);
        }
    }
    public async Task<VKResult> UndeleteAsync(
        string storageName,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(storageName);
        if (!_options.EnableSoftDelete)
        {
            return VKResult.Failure(VKStorageErrors.FeatureDisabled);
        }
        try
        {
            if (!VKStorageGuard.IsValidSafePath(storageName, out var error))
            {
                return VKResult.Failure(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            var containerClient = await GetContainerClientAsync(cancellationToken).ConfigureAwait(false);
            var blobClient = containerClient.GetBlobClient(storageName);
            await blobClient.UndeleteAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogUndeleteFailure(ex, storageName, _containerProvider.GetContainerName());
            return VKResult.Failure(VKStorageErrors.UndeleteFailed);
        }
    }
    public async Task<VKResult<bool>> ExistsAsync(
        string storageName,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(storageName);
        try
        {
            if (!VKStorageGuard.IsValidSafePath(storageName, out var error))
            {
                return VKResult.Failure<bool>(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            var containerClient = await GetContainerClientAsync(cancellationToken).ConfigureAwait(false);
            var blobClient = containerClient.GetBlobClient(storageName);
            var exists = await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success(exists.Value);
        }
        catch (Exception ex)
        {
            _logger.LogExistenceCheckFailure(ex, storageName, _containerProvider.GetContainerName());
            return VKResult.Failure<bool>(VKStorageErrors.GeneralFailure);
        }
    }
    public async Task<VKResult<VKStorageFileMetadata>> GetInfoAsync(
        string storageName,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(storageName);
        try
        {
            if (!VKStorageGuard.IsValidSafePath(storageName, out var error))
            {
                return VKResult.Failure<VKStorageFileMetadata>(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            var containerClient = await GetContainerClientAsync(cancellationToken).ConfigureAwait(false);
            var blobClient = containerClient.GetBlobClient(storageName);
            var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            return VKResult.Success(new VKStorageFileMetadata(
                storageName,
                properties.Value.ContentLength,
                properties.Value.ContentType,
                properties.Value.CreatedOn,
                properties.Value.Metadata));
        }
        catch (Exception ex)
        {
            _logger.LogGetInfoFailure(ex, storageName, _containerProvider.GetContainerName());
            return VKResult.Failure<VKStorageFileMetadata>(VKStorageErrors.GeneralFailure);
        }
    }
    public async Task<VKResult<IReadOnlyList<string>>> ListVersionsAsync(
        string storageName,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(storageName);
        if (!_options.EnableVersioning)
        {
            return VKResult.Failure<IReadOnlyList<string>>(VKStorageErrors.FeatureDisabled);
        }
        try
        {
            if (!VKStorageGuard.IsValidSafePath(storageName, out var error))
            {
                return VKResult.Failure<IReadOnlyList<string>>(new VKError(VKStorageErrors.InvalidPath.Code, error, VKErrorType.Validation));
            }
            var containerClient = await GetContainerClientAsync(cancellationToken).ConfigureAwait(false);
            var versions = new List<string>();
            await foreach (var item in containerClient.GetBlobsAsync(
                BlobTraits.None,
                BlobStates.Version,
                prefix: storageName,
                cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                if (item.Name == storageName && !string.IsNullOrEmpty(item.VersionId))
                {
                    versions.Add(item.VersionId);
                }
            }
            return VKResult.Success<IReadOnlyList<string>>(versions);
        }
        catch (Exception ex)
        {
            _logger.LogListVersionsFailure(ex, storageName, _containerProvider.GetContainerName());
            return VKResult.Failure<IReadOnlyList<string>>(VKStorageErrors.GeneralFailure);
        }
    }
    public VKResult<string> GenerateSasUri(string storageName, VKStorageSasOptions sasOptions)
    {
        VKGuard.NotNullOrWhiteSpace(storageName);
        VKGuard.NotNull(sasOptions);
        try
        {
            var containerName = _containerProvider.GetContainerName();
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(storageName);
            if (!blobClient.CanGenerateSasUri)
            {
                return VKResult.Failure<string>(VKStorageErrors.GeneralFailure);
            }
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = storageName,
                Resource = "b",
                ExpiresOn = _timeProvider.GetUtcNow().Add(sasOptions.ExpiresIn)
            };
            BlobSasPermissions permissions = 0;
            if (sasOptions.Permissions.HasFlag(VKStoragePermissions.Read))
                permissions |= BlobSasPermissions.Read;
            if (sasOptions.Permissions.HasFlag(VKStoragePermissions.Write))
                permissions |= BlobSasPermissions.Write;
            if (sasOptions.Permissions.HasFlag(VKStoragePermissions.Delete))
                permissions |= BlobSasPermissions.Delete;
            sasBuilder.SetPermissions(permissions);
            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return VKResult.Success(sasUri.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogSasUriGenerationFailure(ex, storageName, _containerProvider.GetContainerName());
            return VKResult.Failure<string>(VKStorageErrors.GeneralFailure);
        }
    }
    public VKResult<string> GetPublicUrl(string storageName)
    {
        VKGuard.NotNullOrWhiteSpace(storageName);
        var containerName = _containerProvider.GetContainerName();
        var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(storageName);
        return VKResult.Success(blobClient.Uri.ToString());
    }
    private async Task<BlobContainerClient> GetContainerClientAsync(CancellationToken cancellationToken)
    {
        var containerName = _containerProvider.GetContainerName();
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return containerClient;
    }
}
