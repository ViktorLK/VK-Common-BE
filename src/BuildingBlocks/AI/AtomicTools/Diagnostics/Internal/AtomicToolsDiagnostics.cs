using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.AtomicTools.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIBlock>]
internal static partial class AtomicToolsDiagnostics
{
    [LoggerMessage(
        EventId = VKAtomicToolsDiagnosticTokens.AtomicToolsInitializedEventId,
        Level = LogLevel.Debug,
        Message = "AtomicTools feature initialized.")]
    public static partial void AtomicToolsInitialized(ILogger logger);
}
