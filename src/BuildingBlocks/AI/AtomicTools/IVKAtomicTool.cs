using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the interface for an atomic tool/function that can be called by AI.
/// Renamed from IVKAgentTool to reflect its broader use beyond just agents.
/// </summary>
public interface IVKAtomicTool
{
    /// <summary>
    /// Gets the tool manifest (Definition/Schema).
    /// </summary>
    VKAtomicToolManifest Manifest { get; }

    /// <summary>
    /// Executes the tool with the given arguments.
    /// </summary>
    /// <param name="arguments">The tool arguments.</param>
    /// <param name="context">The execution context provided by the caller.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tool execution result.</returns>
    Task<VKResult<VKAtomicToolResult>> ExecuteAsync(
        IDictionary<string, object> arguments,
        VKAgentExecutionContext context,
        CancellationToken cancellationToken = default);

    // TODO: Add ReturnSchema property to support structured output reasoning.
    // TODO: Consider ExecuteStreamingAsync for long-running tools.
}
