using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Embeddings.Internal;

/// <summary>
/// Source-generated logger messages for the Embeddings feature.
/// </summary>
internal static partial class EmbeddingsLog
{
    [LoggerMessage(
        EventId = 300,
        Level = LogLevel.Information,
        Message = "Embeddings generated for {Count} inputs using model: {Model}")]
    public static partial void EmbeddingsGenerated(ILogger logger, int count, string? model);
}
