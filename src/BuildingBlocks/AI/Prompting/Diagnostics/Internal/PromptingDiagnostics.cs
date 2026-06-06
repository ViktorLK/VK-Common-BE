using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Prompting.Diagnostics.Internal;

/// <summary>
/// High-performance logger messages for the Prompting feature.
/// </summary>
[VKBlockDiagnostics<VKAIBlock>]
internal static partial class PromptingDiagnostics
{
    [LoggerMessage(
        EventId = VKPromptingDiagnosticTokens.PromptingInitializedEventId,
        Level = LogLevel.Debug,
        Message = "Prompting feature initialized.")]
    public static partial void PromptingInitialized(ILogger logger);

    [LoggerMessage(
        EventId = 5100,
        Level = LogLevel.Warning,
        Message = "[AI] Prompt template '{PromptId}' (Version: {Version}) was not found in any registered provider.")]
    public static partial void PromptNotFound(ILogger logger, string promptId, string version);
}
