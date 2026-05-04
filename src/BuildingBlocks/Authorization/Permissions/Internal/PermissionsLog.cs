using Microsoft.Extensions.Logging;

namespace VK.Blocks.Authorization.Permissions.Internal;

/// <summary>
/// Source-generated logging for the Permissions feature.
/// </summary>
internal static partial class PermissionsLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Permission '{permission}' granted to user {userId}.")]
    internal static partial void LogPermissionGranted(
        this ILogger logger,
        string permission,
        string userId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Permission '{permission}' denied to user {userId}.")]
    internal static partial void LogPermissionDenied(
        this ILogger logger,
        string permission,
        string userId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Error occurred while checking permission '{permission}' for user {userId}: {errorCode} - {errorMessage}")]
    internal static partial void LogPermissionCheckError(
        this ILogger logger,
        string permission,
        string userId,
        string errorCode,
        string errorMessage);
}
