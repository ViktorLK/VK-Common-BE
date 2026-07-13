using Microsoft.Extensions.Logging;

namespace VK.Blocks.Persistence.Cosmos.Common.Diagnostics.Internal;

/// <summary>
/// Source-generated logger for Cosmos DB operations.
/// </summary>
internal static partial class CosmosLog
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Cosmos operation {OperationName} on container {ContainerName} charged {RequestCharge} RUs.")]
    public static partial void LogRequestCharge(ILogger logger, string operationName, string containerName, double requestCharge);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Warning, Message = "Cosmos concurrency failure on container {ContainerName} for ID {Id}: {Message}")]
    public static partial void LogConcurrencyWarning(ILogger logger, string containerName, string id, string message);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Received {ChangeCount} change notifications from Cosmos Change Feed.")]
    public static partial void LogChangeFeedReceived(ILogger logger, int changeCount);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Information, Message = "Provisioned {IndexCount} composite indexes on container {ContainerName}.")]
    public static partial void LogCompositeIndexProvisioned(ILogger logger, string containerName, int indexCount);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Warning, Message = "Cosmos 429 rate-limit hit on operation {OperationName}. RU charge: {RequestCharge}. Retry attempt {RetryCount}/{MaxRetries}.")]
    public static partial void LogRateLimitHit(ILogger logger, string operationName, double requestCharge, int retryCount, int maxRetries);

    [LoggerMessage(EventId = 1006, Level = LogLevel.Information, Message = "Transactional batch on container {ContainerName} completed: {OperationCount} operations, {RequestCharge} RUs.")]
    public static partial void LogTransactionalBatchCompleted(ILogger logger, string containerName, int operationCount, double requestCharge);

    [LoggerMessage(EventId = 1007, Level = LogLevel.Information, Message = "Session token captured: {SessionToken} for container {ContainerName}.")]
    public static partial void LogSessionTokenCaptured(ILogger logger, string sessionToken, string containerName);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Warning, Message = "Cross-partition query detected on container {ContainerName}. Query: {QueryText}. This may result in high RU consumption.")]
    public static partial void LogCrossPartitionQuery(ILogger logger, string containerName, string queryText);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Information, Message = "Query on container {ContainerName} returned {ItemCount} items. Total RU: {TotalRequestCharge}.")]
    public static partial void LogQueryCompleted(ILogger logger, string containerName, int itemCount, double totalRequestCharge);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Information, Message = "Stored procedure {ProcedureId} executed on container {ContainerName}. RU: {RequestCharge}.")]
    public static partial void LogStoredProcedureExecuted(ILogger logger, string procedureId, string containerName, double requestCharge);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Information, Message = "Container {ContainerName} provisioned with TTL={TtlSeconds}s, AnalyticalStore={AnalyticalStoreEnabled}.")]
    public static partial void LogContainerProvisioned(ILogger logger, string containerName, int? ttlSeconds, bool analyticalStoreEnabled);
}

