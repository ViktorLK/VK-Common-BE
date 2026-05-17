using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.Cognitive.Common.DependencyInjection.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Service collection extensions for the AI Cognitive building block.
/// Public API entry point following BB.03.1 (Wrapper Pattern).
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
}
