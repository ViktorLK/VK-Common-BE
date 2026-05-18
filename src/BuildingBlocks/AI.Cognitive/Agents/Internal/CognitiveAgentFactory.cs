using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Agents.Internal;

/// <summary>
/// Default implementation of the cognitive agent factory.
/// Following AP.01 (Sealed).
/// </summary>
internal sealed class CognitiveAgentFactory : IVKCognitiveAgentFactory
{
    private readonly IServiceProvider _serviceProvider;

    public CognitiveAgentFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = VKGuard.NotNull(serviceProvider);
    }


    /// <inheritdoc />
    public IVKAgent CreateAgent(
        string name,
        string description,
        IEnumerable<IVKAtomicTool> tools,
        string personaId,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        VKGuard.NotNullOrWhiteSpace(name);
        VKGuard.NotNullOrWhiteSpace(description);
        VKGuard.NotNull(tools);
        VKGuard.NotNullOrWhiteSpace(personaId);

        return CreateAgentInternal(name, description, tools, metadata, personaId);
    }

    /// <inheritdoc />
    public IVKAgent CreateCognitiveAgent(
        string name,
        string description,
        IEnumerable<IVKAtomicTool> tools,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        VKGuard.NotNullOrWhiteSpace(name);
        VKGuard.NotNullOrWhiteSpace(description);
        VKGuard.NotNull(tools);

        return CreateAgentInternal(name, description, tools, metadata);
    }

    private IVKAgent CreateAgentInternal(
        string name,
        string description,
        IEnumerable<IVKAtomicTool> tools,
        IReadOnlyDictionary<string, object>? metadata = null,
        string? personaId = null)
    {
        var chatEngine = _serviceProvider.GetRequiredService<IVKChatEngine>();
        var personaCodex = _serviceProvider.GetRequiredService<IVKPersonaCodex>();
        var options = _serviceProvider.GetRequiredService<IOptions<VKAgentsOptions>>();
        var cognitiveAgentsOptions = _serviceProvider.GetRequiredService<IOptions<VKCognitiveAgentsOptions>>().Value;
        var logger = _serviceProvider.GetRequiredService<ILogger<CognitiveAgent>>();
        var filters = _serviceProvider.GetServices<IVKAtomicToolFilter>();

        var effectivePersonaId = personaId ?? cognitiveAgentsOptions.DefaultPersonaId ?? "default";

        return new CognitiveAgent(name, description, tools, metadata, chatEngine, personaCodex, options, logger, effectivePersonaId, filters);
    }
}
