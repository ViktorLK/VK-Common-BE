using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Context provided to <see cref="IVKAtomicToolFilter.OnToolExecutedAsync"/>.
/// </summary>
public sealed record VKAtomicToolExecutedContext(
    IVKAgent Agent,
    IVKAtomicTool Tool,
    IDictionary<string, object> Arguments,
    VKAgentExecutionContext ExecutionContext,
    VKResult<VKAtomicToolResult> Result);
