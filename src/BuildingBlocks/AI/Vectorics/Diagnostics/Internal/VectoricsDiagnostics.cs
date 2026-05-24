using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Vectorics.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIBlock>]
internal static partial class VectoricsDiagnostics
{
    [LoggerMessage(
        EventId = VKVectoricsDiagnosticTokens.RetrievalFailedEventId,
        Level = LogLevel.Warning,
        Message = "RAG Retrieval phase failed. Proceeding without context.")]
    public static partial void RetrievalFailed(ILogger logger);
}
