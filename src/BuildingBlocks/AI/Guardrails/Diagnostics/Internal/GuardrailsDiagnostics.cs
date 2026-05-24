using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Guardrails.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIBlock>]
internal static partial class GuardrailsDiagnostics
{
    [LoggerMessage(
        EventId = VKGuardrailsDiagnosticTokens.ContentFlaggedEventId,
        Level = LogLevel.Warning,
        Message = "Content flagged by moderation engine. Reason: {Reason}")]
    public static partial void ContentFlagged(ILogger logger, string? reason);
}
