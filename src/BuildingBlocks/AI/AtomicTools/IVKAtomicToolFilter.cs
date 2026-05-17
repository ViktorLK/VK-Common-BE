using System.Collections.Generic;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines a filter that can intercept atomic tool execution.
/// </summary>
public interface IVKAtomicToolFilter
{
    /// <summary>
    /// Called before a tool is executed.
    /// </summary>
    Task OnToolExecutingAsync(VKAtomicToolExecutingContext context);

    /// <summary>
    /// Called after a tool has been executed.
    /// </summary>
    Task OnToolExecutedAsync(VKAtomicToolExecutedContext context);
}

/// <summary>
/// Context provided to <see cref="IVKAtomicToolFilter.OnToolExecutingAsync"/>.
/// </summary>
public sealed record VKAtomicToolExecutingContext(
    IVKAgent Agent,
    IVKAtomicTool Tool,
    IDictionary<string, object> Arguments,
    VKAgentExecutionContext ExecutionContext)
{
    /// <summary>
    /// Gets or sets a value indicating whether the tool execution should be canceled by the filter.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets or sets the result to return if execution is canceled.
    /// </summary>
    public VKResult<VKAtomicToolResult>? Result { get; set; }
}

/// <summary>
/// Context provided to <see cref="IVKAtomicToolFilter.OnToolExecutedAsync"/>.
/// </summary>
public sealed record VKAtomicToolExecutedContext(
    IVKAgent Agent,
    IVKAtomicTool Tool,
    IDictionary<string, object> Arguments,
    VKAgentExecutionContext ExecutionContext,
    VKResult<VKAtomicToolResult> Result);
