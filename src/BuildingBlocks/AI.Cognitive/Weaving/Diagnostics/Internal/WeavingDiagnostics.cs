using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Weaving.Diagnostics.Internal;

[VKBlockDiagnostics<VKAICognitiveBlock>]
internal static partial class WeavingDiagnostics
{
    [LoggerMessage(
        EventId = VKWeavingDiagnosticTokens.WeavingInitializedEventId,
        Level = LogLevel.Information,
        Message = "Weaving feature initialized for {Name}")]
    public static partial void WeavingInitialized(ILogger logger, string name);
}
