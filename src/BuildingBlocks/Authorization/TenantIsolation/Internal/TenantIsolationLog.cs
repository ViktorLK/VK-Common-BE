using Microsoft.Extensions.Logging;

namespace VK.Blocks.Authorization.TenantIsolation.Internal;

/// <summary>
/// Source-generated logging for the TenantIsolation feature.
/// Provides detailed diagnostic context for tenant-based authorization.
/// </summary>
internal static partial class TenantIsolationLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Tenant isolation check succeeded for user {UserId}. User Tenant: {UserTenantId}. Target Tenant: {TargetTenantId}. Policy: {PolicyName}.")]
    internal static partial void LogTenantCheckSucceeded(
        this ILogger logger,
        string userId,
        string? userTenantId,
        string? targetTenantId,
        string policyName);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Tenant isolation mismatch for user {UserId}. User Tenant: {UserTenantId}. Target Tenant: {TargetTenantId}. Policy: {PolicyName}.")]
    internal static partial void LogTenantCheckMismatch(
        this ILogger logger,
        string userId,
        string? userTenantId,
        string? targetTenantId,
        string policyName);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Tenant isolation check failed for user {UserId}. Missing User Tenant ID. Policy: {PolicyName}.")]
    internal static partial void LogTenantCheckMissingId(
        this ILogger logger,
        string userId,
        string policyName);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "VKError occurred during tenant isolation check for user {UserId}. Policy: {PolicyName}. VKError: {ErrorCode} - {ErrorMessage}")]
    internal static partial void LogTenantCheckError(
        this ILogger logger,
        string userId,
        string policyName,
        string errorCode,
        string errorMessage);
}
