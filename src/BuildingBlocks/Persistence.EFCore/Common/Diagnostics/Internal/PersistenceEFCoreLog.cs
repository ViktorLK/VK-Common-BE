using Microsoft.Extensions.Logging;

namespace VK.Blocks.Persistence.EFCore.Diagnostics.Internal;

/// <summary>
/// Structured logging for the EF Core Persistence module.
/// </summary>
internal static partial class PersistenceEFCoreLog
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Successfully executed bulk update for entity '{EntityName}'. Rows affected: {RowsAffected}")]
    public static partial void LogBulkUpdateSuccess(this ILogger logger, int rowsAffected, string entityName);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Successfully executed bulk soft delete for entity '{EntityName}'. Rows affected: {RowsAffected}")]
    public static partial void LogBulkSoftDeleteSuccess(this ILogger logger, int rowsAffected, string entityName);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Successfully executed bulk hard delete for entity '{EntityName}'. Rows affected: {RowsAffected}")]
    public static partial void LogBulkDeleteSuccess(this ILogger logger, int rowsAffected, string entityName);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "Switching to schema '{Schema}' for tenant '{TenantId}'")]
    public static partial void LogSwitchingSchema(this ILogger logger, string schema, string tenantId);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Debug,
        Message = "Switching to schema '{Schema}' for tenant '{TenantId}' (Async)")]
    public static partial void LogSwitchingSchemaAsync(this ILogger logger, string schema, string tenantId);
}
