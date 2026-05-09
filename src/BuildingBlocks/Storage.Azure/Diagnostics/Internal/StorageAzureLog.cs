using System;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.Storage.Azure.Diagnostics.Internal;

/// <summary>
/// Structured logging for the Storage Azure block.
/// </summary>
internal static partial class StorageAzureLog
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Storage {storageName} uploaded successfully to container {containerName}.")]
    public static partial void LogUploadSuccess(this ILogger logger, string storageName, string containerName);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Failed to upload storage {storageName} to container {containerName}.")]
    public static partial void LogUploadFailure(this ILogger logger, Exception exception, string storageName, string containerName);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Storage {storageName} deleted from container {containerName}. Mode: {deleteMode}")]
    public static partial void LogDeleteSuccess(this ILogger logger, string storageName, string containerName, string deleteMode);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Failed to delete storage {storageName} from container {containerName}.")]
    public static partial void LogDeleteFailure(this ILogger logger, Exception exception, string storageName, string containerName);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "Failed to generate SAS URI for storage {storageName} in container {containerName}.")]
    public static partial void LogSasUriGenerationFailure(this ILogger logger, Exception exception, string storageName, string containerName);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Error,
        Message = "Failed to check existence for storage {storageName} in container {containerName}.")]
    public static partial void LogExistenceCheckFailure(this ILogger logger, Exception exception, string storageName, string containerName);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Error,
        Message = "Failed directory operation {operation} for {directoryPath} in container {containerName}.")]
    public static partial void LogDirectoryOperationFailure(this ILogger logger, Exception exception, string operation, string directoryPath, string containerName);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Error,
        Message = "Failed tag/metadata operation {operation} for {storageName} in container {containerName}.")]
    public static partial void LogTagOperationFailure(this ILogger logger, Exception exception, string operation, string storageName, string containerName);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Error,
        Message = "Failed lease operation {operation} for {storageName} in container {containerName}.")]
    public static partial void LogLeaseOperationFailure(this ILogger logger, Exception exception, string operation, string storageName, string containerName);

    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Error,
        Message = "Failed container operation {operation} for {containerName}.")]
    public static partial void LogContainerOperationFailure(this ILogger logger, Exception exception, string operation, string containerName);

    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Error,
        Message = "Failed to download storage {storageName} from container {containerName}.")]
    public static partial void LogDownloadFailure(this ILogger logger, Exception exception, string storageName, string containerName);

    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Error,
        Message = "Failed to get properties for storage {storageName} in container {containerName}.")]
    public static partial void LogGetInfoFailure(this ILogger logger, Exception exception, string storageName, string containerName);

    [LoggerMessage(
        EventId = 13,
        Level = LogLevel.Error,
        Message = "Failed to undelete storage {storageName} in container {containerName}.")]
    public static partial void LogUndeleteFailure(this ILogger logger, Exception exception, string storageName, string containerName);

    [LoggerMessage(
        EventId = 14,
        Level = LogLevel.Error,
        Message = "Failed to list versions for storage {storageName} in container {containerName}.")]
    public static partial void LogListVersionsFailure(this ILogger logger, Exception exception, string storageName, string containerName);
}
