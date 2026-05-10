using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Diagnostics.Internal;

/// <summary>
/// Diagnostics for the AISK building block.
/// </summary>
[VKBlockDiagnostics<VKAISKBlock>]
internal static partial class AISKLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "AISK Block initialized for {ModelId}")]
    internal static partial void LogAISKBlockInitialized(this ILogger logger, string modelId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Error occurred during AISK execution: {Error}")]
    internal static partial void LogExecutionError(this ILogger logger, Exception exception, string error);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to route intent for input: {Input}")]
    internal static partial void LogOrchestrationError(this ILogger logger, Exception exception, string input);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Invoking function: {PluginName}.{FunctionName}")]
    internal static partial void LogFunctionInvoking(this ILogger logger, string pluginName, string functionName);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Invoked function: {PluginName}.{FunctionName}")]
    internal static partial void LogFunctionInvoked(this ILogger logger, string pluginName, string functionName);

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = "Prompt rendered for {PluginName}.{FunctionName}:\n{Prompt}")]
    internal static partial void LogPromptRendered(this ILogger logger, string pluginName, string functionName, string prompt);
}
