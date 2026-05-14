using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the interface for a tool/function that an agent can call.
/// </summary>
public interface IVKAgentTool
{
    /// <summary>
    /// Gets the name of the tool.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of what the tool does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Executes the tool with the given arguments.
    /// </summary>
    /// <param name="arguments">The tool arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tool execution result.</returns>
    Task<VKResult<VKAgentToolResult>> ExecuteAsync(
        IDictionary<string, object> arguments,
        CancellationToken cancellationToken = default);
}
