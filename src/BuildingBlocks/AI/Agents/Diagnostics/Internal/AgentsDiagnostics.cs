using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Agents.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIBlock>]
internal static partial class AgentsDiagnostics
{
    [LoggerMessage(
        EventId = VKAgentsDiagnosticsConstants.AgentsInitializedEventId,
        Level = LogLevel.Debug,
        Message = "Agents feature initialized.")]
    public static partial void AgentsInitialized(ILogger logger);

    [LoggerMessage(
        EventId = VKAgentsDiagnosticsConstants.AgentTaskStartedEventId,
        Level = LogLevel.Information,
        Message = "{TenantId} {TraceId} [AI] Agent {Name} started task: {Input}")]
    public static partial void AgentTaskStarted(ILogger logger, string tenantId, string traceId, string name, string input);

    [LoggerMessage(
        EventId = VKAgentsDiagnosticsConstants.AgentTaskCompletedEventId,
        Level = LogLevel.Information,
        Message = "{TenantId} {TraceId} [AI] Agent {Name} completed task with result length: {ResultLength}, Tokens: {Tokens}")]
    public static partial void AgentTaskCompleted(ILogger logger, string tenantId, string traceId, string name, int resultLength, int tokens);

    [LoggerMessage(
        EventId = VKAgentsDiagnosticsConstants.AgentTaskFailedEventId,
        Level = LogLevel.Warning,
        Message = "{TenantId} {TraceId} [AI] Agent {Name} task failed: {Error}")]
    public static partial void AgentTaskFailed(ILogger logger, string tenantId, string traceId, string name, string error);

    [LoggerMessage(
        EventId = VKAgentsDiagnosticsConstants.AgentIterationStartedEventId,
        Level = LogLevel.Debug,
        Message = "{TenantId} {TraceId} [AI] Agent {Name} starting iteration {Iteration}")]
    public static partial void AgentIterationStarted(this ILogger logger, string tenantId, string traceId, string name, int iteration);

    [LoggerMessage(
        EventId = VKAgentsDiagnosticsConstants.ToolCallStartedEventId,
        Level = LogLevel.Debug,
        Message = "{TenantId} {TraceId} [AI] Agent {Name} calling tool {ToolName} with Id {CallId}")]
    public static partial void ToolCallStarted(this ILogger logger, string tenantId, string traceId, string name, string toolName, string callId);

    [LoggerMessage(
        EventId = VKAgentsDiagnosticsConstants.ToolCallCompletedEventId,
        Level = LogLevel.Debug,
        Message = "{TenantId} {TraceId} [AI] Agent {Name} received result for tool {ToolName} with Id {CallId}. Success: {IsSuccess}")]
    public static partial void ToolCallCompleted(this ILogger logger, string tenantId, string traceId, string name, string toolName, string callId, bool isSuccess);

    [LoggerMessage(
        EventId = VKAgentsDiagnosticsConstants.UnexpectedExecutionErrorEventId,
        Level = LogLevel.Error,
        Message = "{TenantId} {TraceId} [AI] Agent {Name} encountered an unexpected execution error.")]
    public static partial void UnexpectedExecutionError(ILogger logger, string tenantId, string traceId, string name, Exception ex);
}
