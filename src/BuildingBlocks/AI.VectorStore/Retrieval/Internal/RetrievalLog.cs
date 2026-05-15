using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.VectorStore.Retrieval.Internal;

/// <summary>
/// Source-generated logger messages for the Retrieval feature.
/// </summary>
internal static partial class RetrievalLog
{
    [LoggerMessage(
        EventId = 500,
        Level = LogLevel.Information,
        Message = "Vector search completed with {Count} results in collection: {Collection}")]
    public static partial void VectorSearchCompleted(ILogger logger, int count, string collection);
}
