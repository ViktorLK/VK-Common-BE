using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Agents.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIBlock>]
internal static partial class AgentsDiagnostics
{
    [LoggerMessage(
        EventId = VKAgentsDiagnosticTokens.AgentsInitializedEventId,
        Level = LogLevel.Debug,
        Message = "Agents feature initialized.")]
    public static partial void AgentsInitialized(ILogger logger);
}
