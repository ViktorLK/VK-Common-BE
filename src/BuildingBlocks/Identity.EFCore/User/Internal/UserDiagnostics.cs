using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.Identity.EFCore.User.Internal;

[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKIdentityEFCoreBlock>]
internal static partial class UserDiagnostics
{
    [LoggerMessage(EventId = 61201, Level = LogLevel.Error, Message = "Failed to get user entity for UserId: {UserId}")]
    public static partial void LogGetUserEntityError(this ILogger logger, Exception ex, string userId);

    [LoggerMessage(EventId = 61202, Level = LogLevel.Error, Message = "Failed to list user entities")]
    public static partial void LogListUserEntitiesError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 61203, Level = LogLevel.Error, Message = "Failed to create user entity for UserId: {UserId}")]
    public static partial void LogCreateUserEntityError(this ILogger logger, Exception ex, string userId);

    [LoggerMessage(EventId = 61204, Level = LogLevel.Error, Message = "Failed to update user entity for UserId: {UserId}")]
    public static partial void LogUpdateUserEntityError(this ILogger logger, Exception ex, string userId);

    [LoggerMessage(EventId = 61205, Level = LogLevel.Error, Message = "Failed to delete user entity for UserId: {UserId}")]
    public static partial void LogDeleteUserEntityError(this ILogger logger, Exception ex, string userId);

    [VKMetricHistogram("vk.identity.efcore.user.duration", Unit = "ms", Description = "Duration of EFCore User database operations in milliseconds.")]
    public static partial void RecordUserOperation(double durationMs, [VKMetricTag("operation")] string operation, [VKMetricTag("success")] bool success);

    [VKMetricCounter("vk.identity.efcore.user.errors", Unit = "errors", Description = "Total number of EFCore User database errors.")]
    public static partial void RecordUserError([VKMetricTag("operation")] string operation);
}
