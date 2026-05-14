using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.Cognitive.DependencyInjection.Internal;
using VK.Blocks.AI.Cognitive.Knowledge.Internal;
using VK.Blocks.AI.Cognitive.Memory.Internal;
using VK.Blocks.AI.Cognitive.Orchestration.Internal;
using VK.Blocks.AI.Cognitive.Persona.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Service collection extensions for the AI Cognitive building block.
/// </summary>
public static class VKAICognitiveBlockExtensions
{
    /// <summary>
    /// Adds the AI Cognitive building block services using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The AI Cognitive block builder.</returns>
    public static IVKAICognitiveBuilder AddVKAICognitiveBlock(this IServiceCollection services, IConfiguration configuration)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        return VKAICognitiveBlockRegistration.Register(services, configuration: configuration);
    }

    /// <summary>
    /// Adds the AI Cognitive building block services using a functional transformation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="transform">The functional transformation to apply to the default options.</param>
    /// <returns>The AI Cognitive block builder.</returns>
    public static IVKAICognitiveBuilder AddVKAICognitiveBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKAICognitiveOptions, VKAICognitiveOptions> transform)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        VKGuard.NotNull(transform);
        return VKAICognitiveBlockRegistration.Register(services, configuration, transform);
    }

    /// <summary>
    /// Adds the Memory feature to the AI Cognitive building block.
    /// </summary>
    public static IVKAICognitiveBuilder AddVKMemory(this IVKAICognitiveBuilder builder)
    {
        VKGuard.NotNull(builder);
        return MemoryFeatureRegistration.Register(builder);
    }

    /// <summary>
    /// Adds the Persona feature to the AI Cognitive building block.
    /// </summary>
    public static IVKAICognitiveBuilder AddVKPersona(this IVKAICognitiveBuilder builder)
    {
        VKGuard.NotNull(builder);
        return PersonaFeatureRegistration.Register(builder);
    }

    /// <summary>
    /// Adds the Knowledge feature to the AI Cognitive building block.
    /// </summary>
    public static IVKAICognitiveBuilder AddVKKnowledge(this IVKAICognitiveBuilder builder)
    {
        VKGuard.NotNull(builder);
        return KnowledgeFeatureRegistration.Register(builder);
    }

    /// <summary>
    /// Adds the Orchestration feature to the AI Cognitive building block.
    /// </summary>
    public static IVKAICognitiveBuilder AddVKOrchestration(this IVKAICognitiveBuilder builder)
    {
        VKGuard.NotNull(builder);
        return OrchestrationFeatureRegistration.Register(builder);
    }

    /// <summary>
    /// Automatically enables all standard AI Cognitive features.
    /// </summary>
    public static IVKAICognitiveBuilder AddVKDefaultFeatures(this IVKAICognitiveBuilder builder)
    {
        VKGuard.NotNull(builder);
        return builder
            .AddVKMemory()
            .AddVKPersona()
            .AddVKKnowledge()
            .AddVKOrchestration();
    }
}
