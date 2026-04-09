using Microsoft.Extensions.Logging;

namespace VK.Blocks.Authorization.Features.Roles;

/// <summary>
/// Source-generated logging for the Roles feature.
/// </summary>
internal static partial class RolesLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Role '{Role}' granted to user {UserId}.")]
    public static partial void LogRoleGranted(
        this ILogger logger,
        string role,
        string userId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Access denied. User {UserId} is missing the required roles: {RequiredRoles}.")]
    public static partial void LogRolesDenied(
        this ILogger logger,
        string userId,
        string requiredRoles);
}
