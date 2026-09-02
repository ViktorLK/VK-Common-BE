using Microsoft.Extensions.Logging;

namespace VK.Blocks.MultiTenancy.EFCore.Common.Diagnostics.Internal;

internal static partial class MultiTenancyEFCoreLog
{
    [LoggerMessage(
        EventId = 5101,
        Level = LogLevel.Information,
        Message = "Switched schema to '{Schema}' for tenant '{TenantId}'.")]
    public static partial void LogSwitchingSchema(this ILogger logger, string schema, string tenantId);

    [LoggerMessage(
        EventId = 5102,
        Level = LogLevel.Information,
        Message = "Switched schema asynchronously to '{Schema}' for tenant '{TenantId}'.")]
    public static partial void LogSwitchingSchemaAsync(this ILogger logger, string schema, string tenantId);
}
