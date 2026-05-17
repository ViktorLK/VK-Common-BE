using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.Common.DependencyInjection.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Service collection extensions for the core AI building block.
/// Public API entry point following BB.03.1 (Wrapper Pattern).
/// </summary>
public static class VKAIBlockExtensions
{
    /// <summary>
    /// Adds the AI building block services using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The AI block builder.</returns>
    public static IVKAIBuilder AddVKAIBlock(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        return AIBlockRegistration.Register(services, configuration);
    }

    /// <summary>
    /// Adds the AI building block services using a functional transformation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="transform">The functional transformation to apply to the default options.</param>
    /// <returns>The AI block builder.</returns>
    public static IVKAIBuilder AddVKAIBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKAIOptions, VKAIOptions> transform)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        VKGuard.NotNull(transform);
        return AIBlockRegistration.Register(services, configuration, transform);
    }
}
