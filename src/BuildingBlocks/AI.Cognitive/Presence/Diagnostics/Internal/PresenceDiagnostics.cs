using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Diagnostics.Internal;

[VKBlockDiagnostics<VKAICognitiveBlock>]
internal static partial class PresenceDiagnostics
{
    [LoggerMessage(
        EventId = VKPresenceDiagnosticTokens.PresenceInitializedEventId,
        Level = LogLevel.Information,
        Message = "Presence feature initialized for {Name}")]
    public static partial void PresenceInitialized(ILogger logger, string name);
}
