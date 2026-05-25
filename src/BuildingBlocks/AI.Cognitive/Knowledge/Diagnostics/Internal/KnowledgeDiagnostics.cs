using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Knowledge.Diagnostics.Internal;

[VKBlockDiagnostics<VKAICognitiveBlock>]
internal static partial class KnowledgeDiagnostics
{
    [LoggerMessage(
        EventId = VKKnowledgeDiagnosticTokens.KnowledgeInitializedEventId,
        Level = LogLevel.Information,
        Message = "Knowledge feature initialized for {Name}")]
    public static partial void KnowledgeInitialized(ILogger logger, string name);
}
