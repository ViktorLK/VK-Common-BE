using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.SemanticKernel.DependencyInjection.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel;

/// <summary>
/// Extension methods for registering the Semantic Kernel building block.
/// </summary>
public static class VKAISKBlockExtensions
{
    /// <summary>
    /// Adds the Semantic Kernel building block to the service collection using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The builder to continue configuration.</returns>
    public static IVKAISKBuilder AddVKAISKBlock(this IServiceCollection services, IConfiguration configuration)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        return AISKBlockRegistration.Register(services, configuration: configuration);
    }

    /// <summary>
    /// Adds the Semantic Kernel building block to the service collection using a functional transformation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">The functional transformation to apply to the default options.</param>
    /// <returns>The builder to continue configuration.</returns>
    public static IVKAISKBuilder AddVKAISKBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKAISKOptions, VKAISKOptions> configure)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        VKGuard.NotNull(configure);
        return AISKBlockRegistration.Register(services, configuration, configure);
    }
}
