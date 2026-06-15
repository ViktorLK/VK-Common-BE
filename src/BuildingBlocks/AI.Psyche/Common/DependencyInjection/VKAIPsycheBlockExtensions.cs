using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.Psyche.Common.DependencyInjection.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Service collection extensions for the AI Psyche building block.
/// Public API entry point following BB.03.1 (Wrapper Pattern).
/// </summary>
public static class VKAIPsycheBlockExtensions
{
    /// <summary>
    /// Adds the AI Psyche building block services using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The AI Psyche block builder.</returns>
    public static IVKAIPsycheBuilder AddVKPsycheBlock(this IServiceCollection services, IConfiguration configuration)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        return AIPsycheBlockRegistration.Register(services, configuration: configuration);
    }

    /// <summary>
    /// Adds the AI Psyche building block services using a functional transformation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="transform">The functional transformation to apply to the default options.</param>
    /// <returns>The AI Psyche block builder.</returns>
    public static IVKAIPsycheBuilder AddVKPsycheBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKAIPsycheOptions, VKAIPsycheOptions> transform)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        VKGuard.NotNull(transform);
        return AIPsycheBlockRegistration.Register(services, configuration, transform);
    }
}
