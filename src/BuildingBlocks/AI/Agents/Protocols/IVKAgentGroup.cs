using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the contract for executing a multi-agent cooperative workflow.
/// </summary>
public interface IVKAgentGroup
{
    /// <summary>
    /// Executes a collaborative task across a set of agents.
    /// </summary>
    Task<VKResult<VKAgentGroupResult>> ExecuteAsync(
        string input,
        IReadOnlyList<IVKAgent> agents,
        VKAgentGroupOptions? options = null,
        CancellationToken cancellationToken = default);
}
