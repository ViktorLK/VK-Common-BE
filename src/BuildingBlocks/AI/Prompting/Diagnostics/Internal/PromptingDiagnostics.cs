using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Prompting.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIBlock>]
internal static partial class PromptingDiagnostics
{
    [LoggerMessage(
        EventId = VKPromptingDiagnosticTokens.PromptingInitializedEventId,
        Level = LogLevel.Debug,
        Message = "Prompting feature initialized.")]
    public static partial void PromptingInitialized(ILogger logger);
}
