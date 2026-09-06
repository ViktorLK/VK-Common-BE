using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.Functions;

/// <summary>
/// Demo function implementing CosmosDBTrigger to handle Change Feed updates in an isolated Azure Functions worker.
/// </summary>
public sealed partial class VKCosmosTriggerDemo
{
    private readonly ILogger<VKCosmosTriggerDemo> _logger;

    public VKCosmosTriggerDemo(ILogger<VKCosmosTriggerDemo> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Function triggered when changes occur in the monitored container.
    /// Handles change feed processing dynamically in Azure Functions.
    /// </summary>
    [Function("VKCosmosTriggerDemo")]
    public void Run([CosmosDBTrigger(
        databaseName: "VKDb",
        containerName: "OptimizeDocumentDemo",
        Connection = "CosmosDBConnectionString",
        LeaseContainerName = "leases",
        CreateLeaseContainerIfNotExists = true)] IReadOnlyList<MyDocument> documents)
    {
        if (documents != null && documents.Count > 0)
        {
            LogChangesDetected(documents.Count);
            foreach (var doc in documents)
            {
                if (doc.Id != null)
                {
                    LogDocumentUpdated(doc.Id);
                }
            }
        }
    }

    [LoggerMessage(EventId = 3001, Level = LogLevel.Information, Message = "Cosmos DB Change Feed detected {Count} updated documents.")]
    private partial void LogChangesDetected(int count);

    [LoggerMessage(EventId = 3002, Level = LogLevel.Information, Message = "Document updated. ID: {Id}")]
    private partial void LogDocumentUpdated(string id);

    /// <summary>
    /// Nested class representing a simple document structure for trigger binding.
    /// </summary>
    public sealed class MyDocument
    {
        public string? Id { get; set; }
    }
}
