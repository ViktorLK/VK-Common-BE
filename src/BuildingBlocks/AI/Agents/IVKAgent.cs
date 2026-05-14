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
    /// Executes the agent task.
    /// </summary>
    /// <param name="input">The task input/instruction.</param>
    /// <param name="context">The execution context.</param>
    /// <param name="args">The execution arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the agent's work.</returns>
    Task<VKResult<string>> ExecuteAsync(
        string input,
        VKAgentExecutionContext? context = null,
        VKAgentArgs? args = null,
        CancellationToken cancellationToken = default);
}
