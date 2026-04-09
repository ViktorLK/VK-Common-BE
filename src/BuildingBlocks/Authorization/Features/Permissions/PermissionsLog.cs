using Microsoft.Extensions.Logging;

namespace VK.Blocks.Authorization.Features.Permissions;

/// <summary>
/// Source-generated logging for the Permissions feature.
/// </summary>
internal static partial class PermissionsLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Permission '{Permission}' granted to user {UserId}.")]
    public static partial void LogPermissionGranted(
        this ILogger logger,
        string permission,
        string userId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Permission '{Permission}' denied to user {UserId}.")]
    public static partial void LogPermissionDenied(
        this ILogger logger,
        string permission,
        string userId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Error occurred while checking permission '{Permission}' for user {UserId}: {ErrorCode} - {ErrorMessage}")]
    public static partial void LogPermissionCheckError(
        this ILogger logger,
        string permission,
        string userId,
        string errorCode,
        string errorMessage);
}
