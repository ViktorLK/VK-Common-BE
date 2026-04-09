using Microsoft.Extensions.Logging;

namespace VK.Blocks.Authorization.Features.DynamicPolicies;

/// <summary>
/// Source-generated logging for the DynamicPolicies feature.
/// </summary>
internal static partial class DynamicPoliciesLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        EventId = 101,
        Message = "Dynamic authorization succeeded for user {UserId} on requirement {RequirementName}. Operator: {Operator}.")]
    public static partial void LogDynamicAuthorizationSucceeded(
        this ILogger logger,
        string userId,
        string requirementName,
        string @operator);

    [LoggerMessage(
        Level = LogLevel.Warning,
        EventId = 102,
        Message = "Dynamic authorization failed for user {UserId} on requirement {RequirementName}. Operator: {Operator}. Reason: {Reason}")]
    public static partial void LogDynamicAuthorizationFailed(
        this ILogger logger,
        string userId,
        string requirementName,
        string @operator,
        string reason);

    [LoggerMessage(
        Level = LogLevel.Error,
        EventId = 103,
        Message = "Error occurred during dynamic authorization check for user {UserId} on requirement {RequirementName}. Code: {ErrorCode}. Message: {ErrorMessage}")]
    public static partial void LogDynamicAuthorizationError(
        this ILogger logger,
        string userId,
        string requirementName,
        string errorCode,
        string errorMessage);
}
