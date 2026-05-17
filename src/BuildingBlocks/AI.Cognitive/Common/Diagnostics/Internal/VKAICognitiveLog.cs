using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Common.Diagnostics.Internal;

/// <summary>
/// Diagnostics for the AI Cognitive building block.
/// </summary>
[VKBlockDiagnostics<VKAICognitiveBlock>]
internal static partial class VKAICognitiveLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "AI Cognitive Block initialized for {BlockName}")]
    internal static partial void LogVKAICognitiveBlockInitialized(this ILogger logger, string blockName);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Cognitive Agent {Name} started with Persona: {PersonaId}")]
    internal static partial void CognitiveAgentStarted(this ILogger logger, string name, string personaId);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Cognitive Agent {Name} is thinking: {Thought}")]
    internal static partial void CognitiveAgentThinking(this ILogger logger, string name, string thought);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Cognitive Agent {Name} encountered an unexpected execution error.")]
    internal static partial void UnexpectedExecutionError(this ILogger logger, string name, System.Exception ex);
}
