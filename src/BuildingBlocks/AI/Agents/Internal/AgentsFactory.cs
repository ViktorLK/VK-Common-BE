using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        string instructions = "",
        IEnumerable<IVKAtomicTool>? tools = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        VKGuard.NotNullOrWhiteSpace(name); // [AP.01]
        VKGuard.NotNullOrWhiteSpace(description); // [AP.01]

        var chatEngine = _serviceProvider.GetRequiredService<IVKChatEngine>(); // [CS.07]
        var options = _serviceProvider.GetRequiredService<IOptions<VKAgentsOptions>>(); // [CS.07]
        var globalOptions = _serviceProvider.GetRequiredService<IOptions<VKAIDefaultsOptions>>(); // [CS.07]
        var userContext = _serviceProvider.GetRequiredService<IVKUserContext>(); // [CS.07]
        var logger = _serviceProvider.GetRequiredService<ILogger<BasicAgent>>(); // [CS.07]
        var filters = _serviceProvider.GetService<IEnumerable<IVKAtomicToolFilter>>();

        return new BasicAgent(
            name,
            description,
            instructions,
            tools ?? Array.Empty<IVKAtomicTool>(),
            metadata,
            chatEngine,
            options,
            globalOptions,
            userContext,
            logger,
            filters);
    }

    /// <inheritdoc />
    public IVKAgentGroup CreateAgentGroup()
    {
        throw new NotSupportedException("Cooperative agent group is not supported in the basic agents provider. Use the AI.SemanticKernel provider instead.");
    }
}
