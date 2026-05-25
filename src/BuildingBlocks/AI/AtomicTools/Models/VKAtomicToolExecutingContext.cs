using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

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
