using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.AI.Cognitive;

namespace VK.Blocks.AI.Cognitive.Reasoning.Diagnostics.Internal;

[VKBlockDiagnostics<VKAICognitiveBlock>]
internal static partial class CognitiveReasoningDiagnostics
{
    [LoggerMessage(
        EventId = VKReasoningDiagnosticTokens.GoalDecompositionFailedEventId,
        Level = LogLevel.Error,
        Message = "Failed to decompose goal: {Goal}")]
    public static partial void GoalDecompositionFailed(ILogger logger, Exception ex, string goal);

    [LoggerMessage(
        EventId = VKReasoningDiagnosticTokens.StepParsingFailedEventId,
        Level = LogLevel.Error,
        Message = "Failed to parse decomposed steps.")]
    public static partial void StepParsingFailed(ILogger logger, Exception ex);
}
