using System;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.Authorization.Features.WorkingHours;

/// <summary>
/// Source-generated logging for the WorkingHours feature.
/// </summary>
internal static partial class WorkingHoursLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Working hours authorization succeeded for user {UserId}. Current time {Now} is within window {Start}-{End}. Policy: {PolicyName}.")]
    public static partial void LogAuthorizationSucceeded(
        this ILogger logger,
        string userId,
        TimeOnly now,
        TimeOnly start,
        TimeOnly end,
        string policyName);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Working hours authorization failed for user {UserId}. Current time {Now} is outside window {Start}-{End}. Policy: {PolicyName}.")]
    public static partial void LogAuthorizationFailed(
        this ILogger logger,
        string userId,
        TimeOnly now,
        TimeOnly start,
        TimeOnly end,
        string policyName);
}
