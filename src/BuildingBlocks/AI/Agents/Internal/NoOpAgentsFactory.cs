using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Agents.Internal;

/// <summary>
/// No-op implementation of <see cref="IVKAgentFactory"/> for graceful disablement when no agent provider is registered.
/// </summary>
internal sealed class NoOpAgentsFactory : IVKAgentFactory
{
    /// <inheritdoc />
    public IVKAgent CreateAgent(
        string name,
        string description,
        string instructions = "",
        IEnumerable<IVKAtomicTool>? tools = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        VKGuard.NotNullOrWhiteSpace(name); // [AP.01]
        VKGuard.NotNullOrWhiteSpace(description); // [AP.01]

        return NoOpAgent.Instance;
    }

    /// <inheritdoc />
    public IVKAgentGroup CreateAgentGroup()
    {
        return NoOpAgentGroup.Instance;
    }
}
