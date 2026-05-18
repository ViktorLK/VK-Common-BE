using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Cognitive.Common.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Agents.Internal;

/// <summary>
/// An advanced agent implementation that integrates with Persona and Memory.
/// Following the "Industrial DNA" standards.
/// </summary>
internal sealed class CognitiveAgent : IVKAgent
{
    private readonly IVKChatEngine _chatEngine;
    private readonly IVKPersonaCodex _personaCodex;
    private readonly VKAgentsOptions _options;
    private readonly ILogger<CognitiveAgent> _logger;
    private readonly string? _defaultPersonaId;
    private readonly IReadOnlyList<IVKAtomicToolFilter> _filters;

    public CognitiveAgent(
        string name,
        string description,
        IEnumerable<IVKAtomicTool> tools,
        IReadOnlyDictionary<string, object>? metadata,
        IVKChatEngine chatEngine,
        IVKPersonaCodex personaCodex,
        IOptions<VKAgentsOptions> options,
        ILogger<CognitiveAgent> logger,
        string? defaultPersonaId = null,
        IEnumerable<IVKAtomicToolFilter>? filters = null)
    {
        Name = VKGuard.NotNullOrWhiteSpace(name);
        Description = VKGuard.NotNullOrWhiteSpace(description);
        Tools = VKGuard.NotNull(tools).ToList();
        Metadata = metadata ?? new Dictionary<string, object>();
        _chatEngine = VKGuard.NotNull(chatEngine);
        _personaCodex = VKGuard.NotNull(personaCodex);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
        _defaultPersonaId = defaultPersonaId;
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

        var effectiveOptions = args.Merge(_options);
        int maxIterations = effectiveOptions.MaxIterations ?? 10;
        TimeSpan timeout = effectiveOptions.Timeout ?? TimeSpan.FromSeconds(100);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            // 1. Resolve Persona (if specified in context or args, or default)
            string? personaId = args?.Context.TryGetValue("PersonaId", out var pId) == true ? pId?.ToString() : null;
            personaId ??= context.Variables.TryGetValue("PersonaId", out var vId) ? vId?.ToString() : null;
            personaId ??= _defaultPersonaId;

            var personaResult = personaId != null
                ? await _personaCodex.GetPersonaAsync(personaId, cts.Token).ConfigureAwait(false)
                : null;

            VKAICognitiveLog.CognitiveAgentStarted(_logger, Name, personaId ?? "Default");

            // 2. Prepare History with Persona System Message
            var history = new List<VKChatMessage>();
            if (personaResult?.IsSuccess == true)
            {
                history.Add(VKChatMessage.FromText(VKChatRole.System, personaResult.Value.Description));
            }

            history.Add(VKChatMessage.FromText(VKChatRole.User, input));

            int iteration = 0;
            while (iteration < maxIterations)
            {
                iteration++;

                var chatArgs = new VKChatArgs
                {
                    Tools = Tools
                };

                var chatResult = await _chatEngine.SendAsync(history, chatArgs, cts.Token).ConfigureAwait(false);
                if (!chatResult.IsSuccess)
                {
                    return VKResult.Failure<string>(chatResult.FirstError);
                }

                var chatResponse = chatResult.Value;
                var assistantMessage = chatResponse.Message;

                // Log reasoning if present
                if (!string.IsNullOrWhiteSpace(assistantMessage.ReasoningContent))
                {
                    VKAICognitiveLog.CognitiveAgentThinking(_logger, Name, assistantMessage.ReasoningContent);
                }

                history.Add(assistantMessage);

                if (assistantMessage.ToolCalls == null || !assistantMessage.ToolCalls.Any())
                {
                    return VKResult.Success(assistantMessage.Content);
                }

                // Execute tool calls (parallel if enabled)
                bool allowParallel = effectiveOptions.AllowParallelToolCalls;

                if (allowParallel && assistantMessage.ToolCalls.Count > 1)
                {
                    var toolTasks = assistantMessage.ToolCalls.Select(tc => ExecuteToolAndWrapAsync(tc, history, context, cts.Token));
                    await Task.WhenAll(toolTasks).ConfigureAwait(false);
                }
                else
                {
                    foreach (var toolCall in assistantMessage.ToolCalls)
                    {
                        await ExecuteToolAndWrapAsync(toolCall, history, context, cts.Token).ConfigureAwait(false);
                    }
                }
            }

            return VKResult.Failure<string>(VKAgentErrors.MaxIterationsReached);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            return VKResult.Failure<string>(VKAgentErrors.Timeout);
        }
        catch (Exception ex)
        {
            VKAICognitiveLog.UnexpectedExecutionError(_logger, Name, ex);
            return VKResult.Failure<string>(VKAgentErrors.ExecutionFailed);
        }
    }

    private async Task ExecuteToolAndWrapAsync(
        VKToolCall toolCall,
        List<VKChatMessage> history,
        VKAgentExecutionContext context,
        CancellationToken ct)
    {
        var tool = Tools.FirstOrDefault(t => t.Manifest.Metadata.Name == toolCall.Name);
        VKToolResult vkToolResult;

        if (tool == null)
        {
            vkToolResult = new VKToolResult
            {
                CallId = toolCall.Id,
                Name = toolCall.Name,
                Content = string.Empty,
                IsSuccess = false,
                ErrorMessage = $"Tool '{toolCall.Name}' not found."
            };
        }
        else
        {
            // Apply Pre-execution filters
            var executingContext = new VKAtomicToolExecutingContext(this, tool, toolCall.Arguments, context);
            foreach (var filter in _filters)
            {
                await filter.OnToolExecutingAsync(executingContext).ConfigureAwait(false);
            }

            VKResult<VKAtomicToolResult> toolResult;
            if (executingContext.Cancel && executingContext.Result != null)
            {
                toolResult = executingContext.Result;
            }
            else
            {
                toolResult = await tool.ExecuteAsync(toolCall.Arguments, context, ct).ConfigureAwait(false);
            }

            // Apply Post-execution filters
            var executedContext = new VKAtomicToolExecutedContext(this, tool, toolCall.Arguments, context, toolResult);
            foreach (var filter in _filters)
            {
                await filter.OnToolExecutedAsync(executedContext).ConfigureAwait(false);
            }

            toolResult = executedContext.Result;

            vkToolResult = new VKToolResult
            {
                CallId = toolCall.Id,
                Name = tool.Manifest.Metadata.Name,
                Content = toolResult.IsSuccess ? toolResult.Value.Content : string.Empty,
                IsSuccess = toolResult.IsSuccess,
                ErrorMessage = toolResult.IsSuccess ? null : toolResult.FirstError.Description
            };

            // Update context
            lock (context.ToolCallHistory)
            {
                context.ToolCallHistory.Add(toolResult.IsSuccess ? toolResult.Value : new VKAtomicToolResult
                {
                    Content = string.Empty
                });
            }
        }

        lock (history)
        {
            history.Add(new VKChatMessage { Role = VKChatRole.Tool, ToolResult = vkToolResult });
        }
    }
}
