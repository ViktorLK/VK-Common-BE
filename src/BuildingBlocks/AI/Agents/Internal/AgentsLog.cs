using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Agents.Internal;

/// <summary>
/// Source-generated logger messages for the Agents feature.
/// </summary>
internal static partial class AgentsLog
{
    [LoggerMessage(
        EventId = 600,
        Level = LogLevel.Information,
        Message = "Agent {Name} started task: {Input}")]
    public static partial void AgentTaskStarted(ILogger logger, string name, string input);

    [LoggerMessage(
        EventId = 601,
        Level = LogLevel.Information,
        Message = "Agent {Name} completed task with result: {Result}")]
    public static partial void AgentTaskCompleted(ILogger logger, string name, string result);
}
