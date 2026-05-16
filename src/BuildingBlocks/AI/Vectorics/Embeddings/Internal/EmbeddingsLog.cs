using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Vectorics.Embeddings.Internal;

/// <summary>
/// Source-generated logger messages for the Embeddings feature.
/// </summary>
// [SG Logger] - This class is automatically implemented by the Source Generator for high-performance logging.
internal static partial class EmbeddingsLog
{
    [LoggerMessage(
        EventId = 300,
        Level = LogLevel.Information,
        Message = "Embeddings generated for {Count} inputs using model: {Model}")]
    public static partial void EmbeddingsGenerated(ILogger logger, int count, string? model);
}
