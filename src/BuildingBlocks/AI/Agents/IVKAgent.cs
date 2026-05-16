using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;


/// <summary>
/// Defines the interface for an intelligent agent.
/// </summary>
public interface IVKAgent
{
    /// <summary>
    /// Gets the agent name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the agent description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the list of tools available to this agent.
    /// </summary>
    IReadOnlyList<IVKAtomicTool> Tools { get; }

    /// <summary>
    /// Gets the agent metadata.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Executes the agent task.
    /// </summary>
    /// <param name="input">The task input/instruction.</param>
    /// <param name="context">The execution context (history and variables).</param>
    /// <param name="args">The execution arguments (overrides).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the agent's work.</returns>
    Task<VKResult<string>> ExecuteAsync(
        string input,
        VKAgentExecutionContext? context = null,
        VKAgentsArgs? args = null,
        CancellationToken cancellationToken = default);
}
