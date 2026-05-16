using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Agents.Internal;

/// <summary>
/// Default implementation of the agents factory.
/// Following AP.03 (Plural Alignment) for internal module implementation.
/// </summary>
internal sealed class AgentsFactory : IVKAgentFactory
{
    private readonly IServiceProvider _serviceProvider;

    public AgentsFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = VKGuard.NotNull(serviceProvider);
    }

    /// <inheritdoc />
    public IVKAgent CreateAgent(
        string name,
        string description,
        IEnumerable<IVKAtomicTool> tools,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        VKGuard.NotNullOrWhiteSpace(name);
        VKGuard.NotNullOrWhiteSpace(description);
        VKGuard.NotNull(tools);

        // Implementation details...
        return null!; // Placeholder
    }
}
