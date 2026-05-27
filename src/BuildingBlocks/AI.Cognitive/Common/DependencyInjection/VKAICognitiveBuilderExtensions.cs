using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Cognitive.Common.DependencyInjection.Internal;
using VK.Blocks.AI.Cognitive.Knowledge.Internal;
using VK.Blocks.AI.Cognitive.Memory.Internal;
using VK.Blocks.AI.Cognitive.Orchestration.Internal;
using VK.Blocks.AI.Cognitive.Persona.Internal;
using VK.Blocks.AI.Cognitive.Presence.Internal;
using VK.Blocks.AI.Cognitive.Reasoning.Internal;
using VK.Blocks.AI.Cognitive.Weaving.Internal;
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
        AICognitiveDefaultsFeature.Register(builder, transform);
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
        MemoryFeature.Register(builder, transform);
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
        PersonaFeature.Register(builder, transform);
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
        KnowledgeFeature.Register(builder, transform);
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
        // services.TryAddSingleton<IVKKnowledgeNarrativeStore, InMemoryKnowledgeNarrativeStore>();
        // services.TryAddScoped<IVKKnowledgeSessionStateStore, InMemoryKnowledgeSessionStateStore>();
        // services.TryAddScoped<IVKKnowledgeSessionProvider, BasicKnowledgeSessionProvider>();

        // // Decorate the core IVKKnowledgeManager with the Narrative rules decorator
        // services.Decorate<IVKKnowledgeStore, BasicKnowledgeNarrativeStore>();

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
        OrchestrationFeature.Register(builder, transform);
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
        ReasoningFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Core Presence feature to the AI Cognitive building block.
    /// </summary>
    public static IVKAICognitiveBuilder AddVKPresence(
        this IVKAICognitiveBuilder builder,
        Func<VKPresenceOptions, VKPresenceOptions>? transform = null)
    {
        VKGuard.NotNull(builder);
        PresenceFeature.Register(builder, transform);
        return builder;
    }

    /// <summary>
    /// Adds the Prompt Weaving Engine and Pipeline to the AI Cognitive building block.
    /// </summary>
    public static IVKAICognitiveBuilder AddVKWeaving(this IVKAICognitiveBuilder builder, Action<VKWeavingOptions>? configure = null)
    {
        VKGuard.NotNull(builder);
        WeavingFeature.Register(builder, configure);
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

            // 🔴 Core Hub
            .AddVKPresence()
            .AddVKOrchestration()

            // 🟡 Standard Plugins
            .AddVKReasoning()
            .AddVKWeaving()
            .AddVKKnowledge()

            // 🟢 Business Plugins
            .AddVKMemory()
            .AddVKPersona();
    }

    public static IVKAICognitiveBuilder ReplaceExtractor<TDefault, TReplacement>(this IVKAICognitiveBuilder builder)
        where TDefault : class, IVKPromptExtractor
        where TReplacement : class, IVKPromptExtractor
    {
        return ReplaceImplementationInternal<IVKPromptExtractor, TDefault, TReplacement>(builder);
    }

    public static IVKAICognitiveBuilder ReplacePipelineStage<TDefault, TReplacement>(this IVKAICognitiveBuilder builder)
        where TDefault : class, IVKOrchestrationPipelineStage
        where TReplacement : class, IVKOrchestrationPipelineStage
    {
        return ReplaceImplementationInternal<IVKOrchestrationPipelineStage, TDefault, TReplacement>(builder);
    }

    public static IVKAICognitiveBuilder ReplaceFormatter<TDefault, TReplacement>(this IVKAICognitiveBuilder builder)
        where TDefault : class, IVKPromptFormatter
        where TReplacement : class, IVKPromptFormatter
    {
        return ReplaceImplementationInternal<IVKPromptFormatter, TDefault, TReplacement>(builder);
    }

    private static IVKAICognitiveBuilder ReplaceImplementationInternal<TService, TDefault, TReplacement>(IVKAICognitiveBuilder builder)
        where TService : class
        where TDefault : class, TService
        where TReplacement : class, TService
    {
        VKGuard.NotNull(builder);

        var defaultDescriptor = builder.Services.FirstOrDefault(d =>
            d.ServiceType == typeof(TService) &&
            (d.ImplementationType == typeof(TDefault) ||
             d.ImplementationInstance?.GetType() == typeof(TDefault)))
            ?? throw new InvalidOperationException($"Default implementation of type {typeof(TDefault).Name} for service {typeof(TService).Name} is not registered in the service collection.");

        var replacementDescriptor = ServiceDescriptor.Describe(
            typeof(TService),
            typeof(TReplacement),
             defaultDescriptor.Lifetime);

        builder.Services.Remove(defaultDescriptor);
        builder.Services.TryAddEnumerable(replacementDescriptor);

        return builder;
    }
}
