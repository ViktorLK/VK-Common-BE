using Microsoft.Extensions.Logging;

namespace VK.Blocks.MultiTenancy.Diagnostics.Internal;

/// <summary>
/// Source-generated logging extensions for the MultiTenancy core module.
/// </summary>
internal static partial class MultiTenancyLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Tenant '{TenantId}' successfully resolved and validated.")]
    public static partial void LogTenantResolved(this ILogger logger, string tenantId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Tenant '{TenantId}' could not be found or is inactive.")]
    public static partial void LogTenantNotFound(this ILogger logger, string tenantId);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Current TenantContext set to '{TenantId}'.")]
    public static partial void LogTenantContextSet(this ILogger logger, string tenantId);
}
