using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using VK.Blocks.Blob.Abstractions;
using VK.Blocks.Blob.Constants;
using VK.Blocks.Blob.Guards;
using VK.Blocks.Core.Results;

using VK.Blocks.Blob.Diagnostics;

namespace VK.Blocks.Blob.Services;

public sealed class BlobTagService(
    IBlobContainerProvider containerProvider,
    BlobServiceClient blobServiceClient,
    ILogger<BlobTagService> logger) : IBlobTagService
{
    public async Task<Result> SetTagsAsync(string blobName, IDictionary<string, string> tags, CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(SetTagsAsync));
        activity?.SetTag("blob.name", blobName);
        activity?.SetTag("blob.container", containerProvider.GetContainerName());

        try
        {
            if (!BlobGuard.IsValidSafePath(blobName, out var error))
            {
                return Result.Failure(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            var blobClient = GetBlobClient(blobName);
            await blobClient.SetTagsAsync(tags, cancellationToken: cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set tags for blob {BlobName}.", blobName);
            return Result.Failure(BlobErrors.TagOperationFailed);
        }
    }

    public async Task<Result<IDictionary<string, string>>> GetTagsAsync(string blobName, CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(GetTagsAsync));
        activity?.SetTag("blob.name", blobName);
        activity?.SetTag("blob.container", containerProvider.GetContainerName());

        try
        {
            if (!BlobGuard.IsValidSafePath(blobName, out var error))
            {
                return Result.Failure<IDictionary<string, string>>(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            var blobClient = GetBlobClient(blobName);
            var response = await blobClient.GetTagsAsync(cancellationToken: cancellationToken);
            return Result.Success<IDictionary<string, string>>(response.Value.Tags);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get tags for blob {BlobName}.", blobName);
            return Result.Failure<IDictionary<string, string>>(BlobErrors.GeneralFailure);
        }
    }

    public async Task<Result> SetMetadataAsync(string blobName, IDictionary<string, string> metadata, CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(SetMetadataAsync));
        activity?.SetTag("blob.name", blobName);
        activity?.SetTag("blob.container", containerProvider.GetContainerName());

        try
        {
            if (!BlobGuard.IsValidSafePath(blobName, out var error))
            {
                return Result.Failure(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            var blobClient = GetBlobClient(blobName);
            await blobClient.SetMetadataAsync(metadata, cancellationToken: cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set metadata for blob {BlobName}.", blobName);
            return Result.Failure(BlobErrors.TagOperationFailed);
        }
    }

    public async Task<Result<IDictionary<string, string>>> GetMetadataAsync(string blobName, CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(GetMetadataAsync));
        activity?.SetTag("blob.name", blobName);
        activity?.SetTag("blob.container", containerProvider.GetContainerName());

        try
        {
            if (!BlobGuard.IsValidSafePath(blobName, out var error))
            {
                return Result.Failure<IDictionary<string, string>>(new Error(BlobErrors.InvalidPath.Code, error, ErrorType.Validation));
            }
            var blobClient = GetBlobClient(blobName);
            var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            return Result.Success<IDictionary<string, string>>(properties.Value.Metadata);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get metadata for blob {BlobName}.", blobName);
            return Result.Failure<IDictionary<string, string>>(BlobErrors.GeneralFailure);
        }
    }

    public async Task<Result<IReadOnlyList<string>>> FindBlobsByTagAsync(string tagFilterExpression, CancellationToken cancellationToken = default)
    {
        using var activity = BlobAzureDiagnostics.Source.StartActivity(nameof(FindBlobsByTagAsync));
        activity?.SetTag("blob.tag_filter", tagFilterExpression);

        try
        {
            var blobs = new List<string>();
            await foreach (var item in blobServiceClient.FindBlobsByTagsAsync(tagFilterExpression, cancellationToken: cancellationToken))
            {
                blobs.Add(item.BlobName);
            }
            return Result.Success<IReadOnlyList<string>>(blobs);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to find blobs by tag expression {Expression}.", tagFilterExpression);
            return Result.Failure<IReadOnlyList<string>>(BlobErrors.TagOperationFailed);
        }
    }

    private BlobClient GetBlobClient(string blobName)
    {
        var containerName = containerProvider.GetContainerName();
        return blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
    }
}
