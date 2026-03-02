using Microsoft.Extensions.Logging;

namespace VK.Blocks.Persistence.EFCore.Internal;

/// <summary>
/// Source-generated logger for EF Core repository events.
/// </summary>
internal static partial class EfCoreRepoLogger
{
    #region Public Methods

    /// <summary>
    /// Logs a concurrency warning.
    /// </summary>
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Optimistic concurrency failure for {EntityType} with ID {Id}.")]
    public static partial void LogConcurrencyWarning(this ILogger logger, string entityType, string id);

    /// <summary>
    /// Logs a successful bulk update.
    /// </summary>
    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Bulk update completed. Affected {Count} rows for entity type {EntityType}.")]
    public static partial void LogBulkUpdateSuccess(this ILogger logger, int count, string entityType);

    /// <summary>
    /// Logs a successful bulk soft delete.
    /// </summary>
    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Information,
        Message = "Bulk softdelete completed. Affected {Count} rows for entity type {EntityType}.")]
    public static partial void LogBulkSoftDeleteSuccess(this ILogger logger, int count, string entityType);

    /// <summary>
    /// Logs a successful bulk delete.
    /// </summary>
    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Information,
        Message = "Bulk delete completed. Removed {Count} rows for entity type {EntityType}.")]
    public static partial void LogBulkDeleteSuccess(this ILogger logger, int count, string entityType);

    #endregion
}
