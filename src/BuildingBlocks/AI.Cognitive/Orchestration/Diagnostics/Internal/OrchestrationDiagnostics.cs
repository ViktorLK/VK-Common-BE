using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Orchestration.Diagnostics.Internal;

[VKBlockDiagnostics<VKAICognitiveBlock>]
internal static partial class OrchestrationDiagnostics
{
    [LoggerMessage(
        EventId = VKOrchestrationDiagnosticTokens.IntentRoutingFailedEventId,
        Level = LogLevel.Error,
        Message = "Error routing intent for input: {Input}")]
    public static partial void IntentRoutingFailed(ILogger logger, Exception ex, string input);
}
