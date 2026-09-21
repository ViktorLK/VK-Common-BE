using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.Identity.EFCore.Tenant.Internal;

[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKIdentityEFCoreBlock>]
internal static partial class TenantDiagnostics
{
    [LoggerMessage(EventId = 61101, Level = LogLevel.Error, Message = "Failed to get tenant entity for TenantId: {TenantId}")]
    public static partial void LogGetTenantEntityError(this ILogger logger, Exception ex, string tenantId);

    [LoggerMessage(EventId = 61102, Level = LogLevel.Error, Message = "Failed to list tenant entities")]
    public static partial void LogListTenantEntitiesError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 61103, Level = LogLevel.Error, Message = "Failed to create tenant entity for TenantId: {TenantId}")]
    public static partial void LogCreateTenantEntityError(this ILogger logger, Exception ex, string tenantId);

    [LoggerMessage(EventId = 61104, Level = LogLevel.Error, Message = "Failed to update tenant entity for TenantId: {TenantId}")]
    public static partial void LogUpdateTenantEntityError(this ILogger logger, Exception ex, string tenantId);

    [LoggerMessage(EventId = 61105, Level = LogLevel.Error, Message = "Failed to delete tenant entity for TenantId: {TenantId}")]
    public static partial void LogDeleteTenantEntityError(this ILogger logger, Exception ex, string tenantId);

    [VKMetricHistogram("vk.identity.efcore.tenant.duration", Unit = "ms", Description = "Duration of EFCore Tenant database operations in milliseconds.")]
    public static partial void RecordTenantOperation(double durationMs, [VKMetricTag("operation")] string operation, [VKMetricTag("success")] bool success);

    [VKMetricCounter("vk.identity.efcore.tenant.errors", Unit = "errors", Description = "Total number of EFCore Tenant database errors.")]
    public static partial void RecordTenantError([VKMetricTag("operation")] string operation);
}
