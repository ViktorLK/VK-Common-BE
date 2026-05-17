using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI;
using VK.Blocks.AI.Cognitive.Agents.Internal;
using VK.Blocks.AI.Cognitive.Common.DependencyInjection.Internal;
using VK.Blocks.AI.Cognitive.Knowledge.Internal;
using VK.Blocks.AI.Cognitive.Memory.Internal;
using VK.Blocks.AI.Cognitive.Orchestration.Internal;
using VK.Blocks.AI.Cognitive.Persona.Internal;
using VK.Blocks.AI.Cognitive.Presence.Internal;
using VK.Blocks.AI.Cognitive.Reasoning.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Fluent API extensions for <see cref="IVKAICognitiveBuilder"/> to configure AI Cognitive features.
/// Following the separation of concerns pattern (Block vs Builder extensions).
/// </summary>
public static class VKAICognitiveBuilderExtensions
{
    /// <summary>
    /// Adds AI Cognitive Defaults (DefaultPersonaId, DefaultMinScore, Reasoning defaults) used as fallback for all cognitive features.
    /// </summary>
    public static IVKAICognitiveBuilder AddVKDefaults(
        this IVKAICognitiveBuilder builder,
        Func<VKAICognitiveDefaultsOptions, VKAICognitiveDefaultsOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        var adapter = new CognitiveAIBuilderAdapter(builder.Services, builder.Configuration);
        AICognitiveDefaultsFeature.Register(adapter, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Memory feature to the AI Cognitive building block.
    /// </summary>
    public static IVKAICognitiveBuilder AddVKMemory(
        this IVKAICognitiveBuilder builder,
        Func<VKMemoryOptions, VKMemoryOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        var adapter = new CognitiveAIBuilderAdapter(builder.Services, builder.Configuration);
        MemoryFeature.Register(adapter, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Persona feature to the AI Cognitive building block.
    /// </summary>
    public static IVKAICognitiveBuilder AddVKPersona(
        this IVKAICognitiveBuilder builder,
        Func<VKPersonaOptions, VKPersonaOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        var adapter = new CognitiveAIBuilderAdapter(builder.Services, builder.Configuration);
        PersonaFeature.Register(adapter, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Knowledge feature to the AI Cognitive building block.
    /// </summary>
    public static IVKAICognitiveBuilder AddVKKnowledge(
        this IVKAICognitiveBuilder builder,
        Func<VKKnowledgeOptions, VKKnowledgeOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        var adapter = new CognitiveAIBuilderAdapter(builder.Services, builder.Configuration);
        KnowledgeFeature.Register(adapter, transform);
        return builder;
    }

    /// <summary>
    /// Decorates the core IVKKnowledgeManager with advanced narrative, stochastic and turn-timer rules.
    /// </summary>
    public static IVKAICognitiveBuilder WithVKKnowledgeNarrativeRules(this IVKAICognitiveBuilder builder)
    {
        VKGuard.NotNull(builder);
        var services = builder.Services;

        // Register default thread-safe in-memory stores and providers
        services.TryAddSingleton<IVKKnowledgeNarrativeStore, InMemoryKnowledgeNarrativeStore>();
        services.TryAddScoped<IVKKnowledgeSessionStateStore, InMemoryKnowledgeSessionStateStore>();
        services.TryAddScoped<IVKKnowledgeSessionProvider, DefaultKnowledgeSessionProvider>();

        // Decorate the core IVKKnowledgeManager with the Narrative rules decorator
        services.Decorate<IVKKnowledgeManager, BasicKnowledgeNarrativeManager>();

        return builder;
    }

    /// <summary>
    /// Adds the Orchestration feature to the AI Cognitive building block.
    /// </summary>
    public static IVKAICognitiveBuilder AddVKOrchestration(
        this IVKAICognitiveBuilder builder,
        Func<VKOrchestrationOptions, VKOrchestrationOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        var adapter = new CognitiveAIBuilderAdapter(builder.Services, builder.Configuration);
        OrchestrationFeature.Register(adapter, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Agents feature to the AI Cognitive building block.
    /// </summary>
    public static IVKAICognitiveBuilder AddVKAgents(
        this IVKAICognitiveBuilder builder,
        Func<VKCognitiveAgentsOptions, VKCognitiveAgentsOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        var adapter = new CognitiveAIBuilderAdapter(builder.Services, builder.Configuration);
        CognitiveAgentsFeature.Register(adapter, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Presence feature to the AI Cognitive building block.
    /// </summary>
    public static IVKAICognitiveBuilder AddVKPresence(
        this IVKAICognitiveBuilder builder,
        Func<VKPresenceOptions, VKPresenceOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        var adapter = new CognitiveAIBuilderAdapter(builder.Services, builder.Configuration);
        PresenceFeature.Register(adapter, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Reasoning feature to the AI Cognitive building block.
    /// </summary>
    public static IVKAICognitiveBuilder AddVKReasoning(
        this IVKAICognitiveBuilder builder,
        Func<VKReasoningOptions, VKReasoningOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        var adapter = new CognitiveAIBuilderAdapter(builder.Services, builder.Configuration);
        ReasoningFeature.Register(adapter, transform);
        return builder;
    }

    /// <summary>
    /// Automatically enables all standard AI Cognitive features.
    /// </summary>
    public static IVKAICognitiveBuilder AddVKDefaultFeatures(this IVKAICognitiveBuilder builder)
    {
        VKGuard.NotNull(builder);
        return builder
            .AddVKDefaults()
            .AddVKMemory()
            .AddVKPersona()
            .AddVKKnowledge()
            .AddVKOrchestration()
            .AddVKAgents()
            .AddVKPresence()
            .AddVKReasoning();
    }

    /// <summary>
    /// Adapter class to bridge IVKAICognitiveBuilder to IVKAIBuilder for features using [VKFeature].
    /// </summary>
    private sealed class CognitiveAIBuilderAdapter(IServiceCollection services, IConfiguration? configuration) : IVKAIBuilder
    {
        public IServiceCollection Services { get; } = services;
        public IConfiguration? Configuration { get; } = configuration;
    }
}
