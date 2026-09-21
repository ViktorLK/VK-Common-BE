using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.Identity.EFCore.TenantUser.Internal;

[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKIdentityEFCoreBlock>]
internal static partial class TenantUserDiagnostics
{
    [LoggerMessage(EventId = 61301, Level = LogLevel.Error, Message = "Failed to find tenant user relation for TenantId: {TenantId}, UserId: {UserId}")]
    public static partial void LogFindTenantUserError(this ILogger logger, Exception ex, string tenantId, string userId);

    [LoggerMessage(EventId = 61302, Level = LogLevel.Error, Message = "Failed to list tenant users for UserId: {UserId}")]
    public static partial void LogListByUserError(this ILogger logger, Exception ex, string userId);

    [LoggerMessage(EventId = 61308, Level = LogLevel.Error, Message = "Failed to list tenant users for TenantId: {TenantId}")]
    public static partial void LogListByTenantError(this ILogger logger, Exception ex, string tenantId);

    [LoggerMessage(EventId = 61303, Level = LogLevel.Error, Message = "Failed to count members for TenantId: {TenantId}")]
    public static partial void LogCountByTenantError(this ILogger logger, Exception ex, string tenantId);

    [LoggerMessage(EventId = 61304, Level = LogLevel.Error, Message = "Failed to check existence of tenant user relation for TenantId: {TenantId}, UserId: {UserId}")]
    public static partial void LogExistsError(this ILogger logger, Exception ex, string tenantId, string userId);

    [LoggerMessage(EventId = 61305, Level = LogLevel.Error, Message = "Failed to add tenant user relation for TenantId: {TenantId}, UserId: {UserId}")]
    public static partial void LogAddTenantUserError(this ILogger logger, Exception ex, string tenantId, string userId);

    [LoggerMessage(EventId = 61306, Level = LogLevel.Error, Message = "Failed to update tenant user relation for TenantId: {TenantId}, UserId: {UserId}")]
    public static partial void LogUpdateTenantUserError(this ILogger logger, Exception ex, string tenantId, string userId);

    [LoggerMessage(EventId = 61307, Level = LogLevel.Error, Message = "Failed to remove tenant user relation for TenantId: {TenantId}, UserId: {UserId}")]
    public static partial void LogRemoveTenantUserError(this ILogger logger, Exception ex, string tenantId, string userId);

    [VKMetricHistogram("vk.identity.efcore.tenant_user.duration", Unit = "ms", Description = "Duration of EFCore TenantUser database operations in milliseconds.")]
    public static partial void RecordTenantUserOperation(double durationMs, [VKMetricTag("operation")] string operation, [VKMetricTag("success")] bool success);

    [VKMetricCounter("vk.identity.efcore.tenant_user.errors", Unit = "errors", Description = "Total number of EFCore TenantUser database errors.")]
    public static partial void RecordTenantUserError([VKMetricTag("operation")] string operation);
}
