using System;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Agents.Internal;

/// <summary>
/// Source-generated logger messages for the Agents feature.
/// </summary>
// [SG Logger] - This class is automatically implemented by the Source Generator for high-performance logging.
internal static partial class AgentsLog
{
    [LoggerMessage(
        EventId = 600,
        Level = LogLevel.Information,
        Message = "{TenantId} {TraceId} [AI] Agent {Name} started task: {Input}")]
    public static partial void AgentTaskStarted(ILogger logger, string tenantId, string traceId, string name, string input);

    [LoggerMessage(
        EventId = 601,
        Level = LogLevel.Information,
        Message = "{TenantId} {TraceId} [AI] Agent {Name} completed task with result length: {ResultLength}, Tokens: {Tokens}")]
    public static partial void AgentTaskCompleted(ILogger logger, string tenantId, string traceId, string name, int resultLength, int tokens);

    [LoggerMessage(
        EventId = 602,
        Level = LogLevel.Warning,
        Message = "{TenantId} {TraceId} [AI] Agent {Name} task failed: {Error}")]
    public static partial void AgentTaskFailed(ILogger logger, string tenantId, string traceId, string name, string error);

    [LoggerMessage(
        EventId = 603,
        Level = LogLevel.Debug,
        Message = "{TenantId} {TraceId} [AI] Agent {Name} starting iteration {Iteration}")]
    public static partial void AgentIterationStarted(this ILogger logger, string tenantId, string traceId, string name, int iteration);

    [LoggerMessage(
        EventId = 604,
        Level = LogLevel.Debug,
        Message = "{TenantId} {TraceId} [AI] Agent {Name} calling tool {ToolName} with Id {CallId}")]
    public static partial void ToolCallStarted(this ILogger logger, string tenantId, string traceId, string name, string toolName, string callId);

    [LoggerMessage(
        EventId = 605,
        Level = LogLevel.Debug,
        Message = "{TenantId} {TraceId} [AI] Agent {Name} received result for tool {ToolName} with Id {CallId}. Success: {IsSuccess}")]
    public static partial void ToolCallCompleted(this ILogger logger, string tenantId, string traceId, string name, string toolName, string callId, bool isSuccess);

    [LoggerMessage(
        EventId = 606,
        Level = LogLevel.Error,
        Message = "{TenantId} {TraceId} [AI] Agent {Name} encountered an unexpected execution error.")]
    public static partial void UnexpectedExecutionError(ILogger logger, string tenantId, string traceId, string name, Exception ex);
}
