using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Common.Diagnostics.Internal;
using VK.Blocks.AI.Common.Shared;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Agents.Internal;

/// <summary>
/// A basic agent implementation that provides tool-calling capabilities.
/// Following the "Industrial DNA" standards.
/// </summary>
internal sealed class BasicAgent : IVKAgent
{
    private readonly IVKChatEngine _chatEngine;
    private readonly VKAgentsOptions _options;
    private readonly VKAIDefaultsOptions _globalOptions;
    private readonly IVKUserContext _userContext;
    private readonly ILogger<BasicAgent> _logger;
    private readonly IReadOnlyList<IVKAtomicToolFilter> _filters;

    public BasicAgent(
        string name,
        string description,
        IEnumerable<IVKAtomicTool> tools,
        IReadOnlyDictionary<string, object>? metadata,
        IVKChatEngine chatEngine,
        IOptions<VKAgentsOptions> options,
        IOptions<VKAIDefaultsOptions> globalOptions,
        IVKUserContext userContext,
        ILogger<BasicAgent> logger,
        IEnumerable<IVKAtomicToolFilter>? filters = null)
    {
        Name = VKGuard.NotNullOrWhiteSpace(name);
        Description = VKGuard.NotNullOrWhiteSpace(description);
        Tools = VKGuard.NotNull(tools).ToList();
        Metadata = metadata ?? new Dictionary<string, object>();
        _chatEngine = VKGuard.NotNull(chatEngine);
        _options = VKGuard.NotNull(options?.Value);
        _globalOptions = VKGuard.NotNull(globalOptions?.Value);
        _userContext = VKGuard.NotNull(userContext);
        _logger = VKGuard.NotNull(logger);
        _filters = filters?.ToList() ?? [];
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public IReadOnlyList<IVKAtomicTool> Tools { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object> Metadata { get; }

    /// <inheritdoc />
    public async Task<VKResult<string>> ExecuteAsync(
        string input,
        VKAgentExecutionContext? context = null,
        VKAgentsArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(input);
        context ??= new VKAgentExecutionContext();

        using var activity = AiDiagnostics.Source.StartActivity(VKAIDiagnosticsConstants.Tracing.AgentExecution);
        var traceId = activity?.TraceId.ToString() ?? Activity.Current?.TraceId.ToString() ?? "none";
        var tenantId = _userContext.TenantId ?? "default";

        var sw = Stopwatch.StartNew();
        bool isSuccess = false;

        // 3-Tier Merge: Args (L3) ?? Feature Options (L2) ?? Global AI Options (L1)
        var effectiveOptions = args.Merge(_options);
        var timeout = effectiveOptions.Timeout ?? _globalOptions.Timeout;

        bool enableAudit = (args is IVKAIAuditSettings a ? a.EnableAudit : null) ?? effectiveOptions.EnableAudit ?? _globalOptions.EnableAudit;
        if (enableAudit && _logger.IsEnabled(LogLevel.Information))
        {
            var taskInput = effectiveOptions.LogToolData ? input : "[REDACTED]";
            AgentsLog.AgentTaskStarted(_logger, tenantId, traceId, Name, taskInput);
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            var history = new List<VKChatMessage>();

            // Inject Default System Prompt if provided and not already present (BB.06/AP.01)
            if (!string.IsNullOrWhiteSpace(_options.DefaultSystemPrompt))
            {
                history.Add(VKChatMessage.FromText(VKChatRole.System, _options.DefaultSystemPrompt));
            }

            history.Add(VKChatMessage.FromText(VKChatRole.User, input));

            int iteration = 0;
            int totalTokens = 0;

            while (iteration < effectiveOptions.MaxIterations)
            {
                iteration++;
                AgentsLog.AgentIterationStarted(_logger, tenantId, traceId, Name, iteration);

                // Trim history to stay within context limits (Industrial Safety)
                if (effectiveOptions.MaxHistoryMessages.HasValue && history.Count > effectiveOptions.MaxHistoryMessages.Value)
                {
                    ChatHistoryHelper.TrimHistory(history, effectiveOptions.MaxHistoryMessages.Value);
                }

                // Merge Chat Args (L3) with Chat Options (L2) and inject Tools
                var chatArgs = args?.Chat ?? VKChatArgs.Empty;
                chatArgs = chatArgs with { Tools = Tools };

                var chatResult = await _chatEngine.SendAsync(
                    history,
                    chatArgs,
                    cts.Token).ConfigureAwait(false);
                if (chatResult.IsFailure)
                {
                    if (enableAudit)
                    {
                        AgentsLog.AgentTaskFailed(_logger, tenantId, traceId, Name, chatResult.FirstError.Description);
                    }
                    return VKResult.Failure<string>(chatResult.FirstError);
                }

                var chatResponse = chatResult.Value;
                var assistantMessage = chatResponse.Message;

                // Track and enforce token budget (Cost Control)
                if (chatResponse.Usage is not null)
                {
                    totalTokens += (int)chatResponse.Usage.TotalTokens;
                    AiDiagnostics.RecordTokenUsage(effectiveOptions.Provider?.ToString() ?? "unknown", effectiveOptions.ModelId ?? "unknown", (long)chatResponse.Usage.TotalTokens, tenantId: tenantId);

                    if (effectiveOptions.MaxTotalTokens.HasValue && totalTokens > effectiveOptions.MaxTotalTokens.Value)
                    {
                        AgentsLog.AgentTaskFailed(_logger, tenantId, traceId, Name, "Token budget exceeded.");
                        return VKResult.Failure<string>(VKAgentErrors.ExecutionFailed);
                    }
                }

                history.Add(assistantMessage);

                if (assistantMessage.ToolCalls is null || !assistantMessage.ToolCalls.Any())
                {
                    // Task completed (no more tools to call)
                    isSuccess = true;
                    if (enableAudit)
                    {
                        AgentsLog.AgentTaskCompleted(_logger, tenantId, traceId, Name, assistantMessage.Content.Length, totalTokens);
                    }
                    return VKResult.Success(assistantMessage.Content);
                }

                // Execute tool calls with throttling
                var toolCallsToExecute = assistantMessage.ToolCalls.Take(effectiveOptions.MaxToolCallsPerIteration).ToList();

                if (effectiveOptions.AllowParallelToolCalls)
                {
                    var tasks = toolCallsToExecute.Select(tc => ExecuteToolWithRetryAsync(tc, context, history, effectiveOptions, tenantId, traceId, cts.Token));
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                else
                {
                    foreach (var toolCall in toolCallsToExecute)
                    {
                        await ExecuteToolWithRetryAsync(toolCall, context, history, effectiveOptions, tenantId, traceId, cts.Token).ConfigureAwait(false);
                    }
                }
            }

            if (enableAudit)
            {
                AgentsLog.AgentTaskFailed(_logger, tenantId, traceId, Name, VKAgentErrors.MaxIterationsReached.Description);
            }
            return VKResult.Failure<string>(VKAgentErrors.MaxIterationsReached);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            return VKResult.Failure<string>(VKAgentErrors.Timeout);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            AgentsLog.UnexpectedExecutionError(_logger, tenantId, traceId, Name, ex);
            return VKResult.Failure<string>(VKAgentErrors.ExecutionFailed);
        }
        finally
        {
            sw.Stop();
            AiDiagnostics.RecordAgentRun(Name, isSuccess, sw.Elapsed.TotalMilliseconds, effectiveOptions.ModelId, tenantId);
        }
    }

    private async Task ExecuteToolWithRetryAsync(
        VKToolCall toolCall,
        VKAgentExecutionContext context,
        List<VKChatMessage> history,
        VKAgentsOptions options,
        string tenantId,
        string traceId,
        CancellationToken cancellationToken)
    {
        var tool = Tools.FirstOrDefault(t => t.Manifest.Metadata.Name == toolCall.Name);
        if (tool is null)
        {
            AddToolErrorToHistory(history, toolCall.Id, toolCall.Name, $"Tool '{toolCall.Name}' not found.");
            return;
        }

        AgentsLog.ToolCallStarted(_logger, tenantId, traceId, Name, tool.Manifest.Metadata.Name, toolCall.Id);

        // Implementation of Tool-level retry with 3-tier fallback
        var toolResult = VKResult.Failure<VKAtomicToolResult>(VKAgentErrors.ExecutionFailed);
        int retryCount = options.ToolRetryCount ?? _globalOptions.RetryCount;
        int attempt = 0;

        while (attempt <= retryCount)
        {
            attempt++;

            // Apply Pre-execution filters
            var executingContext = new VKAtomicToolExecutingContext(this, tool, toolCall.Arguments, context);
            foreach (var filter in _filters)
            {
                await filter.OnToolExecutingAsync(executingContext).ConfigureAwait(false);
            }

            if (executingContext.Cancel && executingContext.Result is not null)
            {
                toolResult = executingContext.Result;
            }
            else
            {
                toolResult = await tool.ExecuteAsync(toolCall.Arguments, context, cancellationToken).ConfigureAwait(false);
            }

            // Apply Post-execution filters
            var executedContext = new VKAtomicToolExecutedContext(this, tool, toolCall.Arguments, context, toolResult);
            foreach (var filter in _filters)
            {
                await filter.OnToolExecutedAsync(executedContext).ConfigureAwait(false);
            }

            toolResult = executedContext.Result;

            if (toolResult.IsSuccess || attempt > retryCount)
            {
                break;
            }

            // Exponential Backoff or simple delay (Industrial Reliability)
            if (options.ToolRetryBackoffMs > 0)
            {
                await Task.Delay(options.ToolRetryBackoffMs, cancellationToken).ConfigureAwait(false);
            }
        }

        string content = toolResult.IsSuccess ? toolResult.Value.Content : string.Empty;

        // Truncate large tool results to prevent token explosion (Safety Guard)
        if (options.MaxToolResultLength.HasValue && content.Length > options.MaxToolResultLength.Value)
        {
            content = content[..options.MaxToolResultLength.Value] + "... [TRUNCATED]";
        }

        var vkToolResult = new VKToolResult
        {
            CallId = toolCall.Id,
            Name = tool.Manifest.Metadata.Name,
            Content = content,
            IsSuccess = toolResult.IsSuccess,
            ErrorMessage = toolResult.IsSuccess ? null : toolResult.FirstError.Description
        };

        AgentsLog.ToolCallCompleted(_logger, tenantId, traceId, Name, tool.Manifest.Metadata.Name, toolCall.Id, toolResult.IsSuccess);
        AiDiagnostics.RecordToolCall(Name, tool.Manifest.Metadata.Name, toolResult.IsSuccess, tenantId);

        lock (history)
        {
            history.Add(new VKChatMessage { Role = VKChatRole.Tool, ToolResult = vkToolResult });

            if (toolResult.IsSuccess)
            {
                context.ToolCallHistory.Add(toolResult.Value);
            }
        }
    }

    private static void AddToolErrorToHistory(List<VKChatMessage> history, string callId, string toolName, string errorMessage)
    {
        var errorResult = new VKToolResult
        {
            CallId = callId,
            Name = toolName,
            Content = string.Empty,
            IsSuccess = false,
            ErrorMessage = errorMessage
        };

        lock (history)
        {
            history.Add(new VKChatMessage { Role = VKChatRole.Tool, ToolResult = errorResult });
        }
    }


}
