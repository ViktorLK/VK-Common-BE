using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Text.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIBlock>]
internal static partial class TextDiagnostics
{
    [LoggerMessage(
        EventId = VKTextDiagnosticTokens.TextInitializedEventId,
        Level = LogLevel.Debug,
        Message = "Text feature initialized.")]
    public static partial void TextInitialized(ILogger logger);
}
