using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Shared pure-VK execution pipeline for atomic tool selection, approval checks,
/// <see cref="IVKAtomicToolFilter"/> onion middleware execution, result truncation, and formatting.
/// Adheres to ARCH-01, ARCH-02, ARCH-11, CS.01, and CS.03.
/// </summary>
public static class VKAtomicToolPipeline
{
    private const string TruncationSuffix = "... [truncated]";

    /// <summary>
    /// Selects tools according to explicit per-call/per-agent declarations and optional whitelist filtering.
    /// </summary>
    /// <param name="explicitTools">Tools explicitly provided on the request or agent definition.</param>
    /// <param name="registeredTools">DI-registered tools available in scope.</param>
    /// <param name="enabledToolNames">Optional allow-list of permitted tool names.</param>
    /// <returns>The selected list of <see cref="IVKAtomicTool"/> instances.</returns>
    public static IReadOnlyList<IVKAtomicTool> SelectTools(
        IEnumerable<IVKAtomicTool>? explicitTools,
        IEnumerable<IVKAtomicTool>? registeredTools = null,
        IReadOnlyList<string>? enabledToolNames = null)
    {
        var selected = new List<IVKAtomicTool>();
        var explicitList = explicitTools as IReadOnlyCollection<IVKAtomicTool> ?? explicitTools?.ToList();

        if (explicitList is { Count: > 0 })
        {
            foreach (var tool in explicitList)
            {
                if (tool is null)
                {
                    continue;
                }

                if (enabledToolNames is null ||
                    enabledToolNames.Count == 0 ||
                    IsToolEnabled(tool, enabledToolNames))
                {
                    selected.Add(tool);
                }
            }

            return selected;
        }

        if (enabledToolNames is { Count: > 0 } && registeredTools is not null)
        {
            foreach (var registeredTool in registeredTools)
            {
                if (registeredTool is not null && IsToolEnabled(registeredTool, enabledToolNames))
                {
                    selected.Add(registeredTool);
                }
            }
        }

        return selected;
    }

    /// <summary>
    /// Executes an atomic tool through confirmation checks, sorted <see cref="IVKAtomicToolFilter"/> middleware,
    /// output length governance, and invocation history tracking.
    /// </summary>
    /// <param name="tool">The atomic tool to execute.</param>
    /// <param name="arguments">Normalized arguments passed by the caller or model.</param>
    /// <param name="context">Optional execution context for caller identity and call history.</param>
    /// <param name="filters">Optional onion-style execution filters.</param>
    /// <param name="approvalHandler">Optional confirmation handler for sensitive tools.</param>
    /// <param name="maxResultLength">Optional character cap for returned tool output.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The governed tool execution result.</returns>
    public static async Task<VKResult<VKAtomicToolResult>> ExecuteWithFiltersAsync(
        IVKAtomicTool tool,
        IDictionary<string, object> arguments,
        VKAtomicToolContext? context = null,
        IEnumerable<IVKAtomicToolFilter>? filters = null,
        IVKAtomicToolApprovalHandler? approvalHandler = null,
        int? maxResultLength = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(tool);
        VKGuard.NotNull(arguments);

        var execContext = context ?? new VKAtomicToolContext();
        var invocationContext = new VKAtomicToolInvocationContext(tool, arguments, execContext);

        // [AP.06] Sensitive tools require explicit confirmation and fail closed when unconfigured.
        if (tool.Manifest.Metadata.ApprovalMode != VKToolApprovalMode.Never)
        {
            if (approvalHandler is null)
            {
                return VKResult.Failure<VKAtomicToolResult>(VKAIErrors.ToolApprovalRequired);
            }

            var approvalResult = await approvalHandler.ApproveAsync(invocationContext, cancellationToken).ConfigureAwait(false);
            if (!approvalResult.IsSuccess)
            {
                return VKResult.Failure<VKAtomicToolResult>(approvalResult.FirstError);
            }

            if (!approvalResult.Value)
            {
                return VKResult.Failure<VKAtomicToolResult>(VKAIErrors.ToolApprovalDenied);
            }
        }

        VKAtomicToolExecutionDelegate pipeline = (ctx, ct) => ctx.Tool.ExecuteAsync(ctx.Arguments, ctx.Context, ct);

        if (filters is not null)
        {
            var orderedFilters = filters.Where(static f => f is not null).OrderBy(static f => f.Order).ToList();
            for (int i = orderedFilters.Count - 1; i >= 0; i--)
            {
                var filter = orderedFilters[i];
                var next = pipeline;
                pipeline = (ctx, ct) => filter.InvokeAsync(ctx, next, ct);
            }
        }

        var result = await pipeline(invocationContext, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Value is null)
        {
            return result;
        }

        var finalValue = result.Value;
        if (maxResultLength is > 0 && finalValue.Content.Length > maxResultLength.Value)
        {
            finalValue = finalValue with
            {
                Content = string.Concat(finalValue.Content.AsSpan(0, maxResultLength.Value), TruncationSuffix)
            };
        }

        lock (execContext.ToolCallHistory)
        {
            execContext.ToolCallHistory.Add(finalValue);
        }

        return ReferenceEquals(finalValue, result.Value) ? result : VKResult.Success(finalValue);
    }

    /// <summary>
    /// Formats a tool execution result into a safe model-facing string without leaking infrastructure exceptions.
    /// </summary>
    /// <param name="result">The typed execution result.</param>
    /// <returns>The formatted string for the model.</returns>
    public static string FormatToolResult(VKResult<VKAtomicToolResult> result)
    {
        if (!result.IsSuccess)
        {
            if (result.FirstError.Type is VKErrorType.Validation or VKErrorType.NotFound or VKErrorType.Conflict or VKErrorType.Forbidden)
            {
                return $"Error [{result.FirstError.Code}]: {result.FirstError.Description}";
            }

            return "Error [AI.Tool.InternalFailure]: The tool encountered an internal infrastructure error and could not complete.";
        }

        if (result.Value?.IsToolError == true)
        {
            return $"ToolError: {result.Value.Content}";
        }

        return result.Value?.Content ?? string.Empty;
    }

    private static bool IsToolEnabled(IVKAtomicTool tool, IReadOnlyList<string> enabledToolNames)
    {
        var toolName = tool.Manifest.Metadata.Name;
        for (int i = 0; i < enabledToolNames.Count; i++)
        {
            if (string.Equals(enabledToolNames[i], toolName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
